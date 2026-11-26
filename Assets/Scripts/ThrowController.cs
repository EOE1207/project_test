using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

// 掛在右手控制器（XR Origin > Camera Offset > Right Controller）上
public class ThrowController : MonoBehaviour
{
    public enum State { Ready, Aiming, Flying }

    [Header("輸入（指到 XRI Default Input Actions）")]
    public InputActionReference triggerAction; // XRI Right Interaction/Activate Value
    public InputActionReference gripAction;    // XRI Right Interaction/Select

    [Header("設定")]
    public Transform holdPoint;                // 工具在手上的位置（控制器底下的空物件）
    public ToolData currentTool;
    [Range(0.03f, 0.3f)] public float sampleWindow = 0.1f;   // GDD 5.3 速度窗口
    public float minThrowSpeed = 0.5f;         // 誤觸保護：低於此速度視為取消（m/s）
    public float triggerPressThreshold = 0.7f;
    public float triggerReleaseThreshold = 0.3f;

    // 給程式 B 的狀態機訂閱：true = 有命中（含擊殺），false = 未命中
    public event Action<bool> OnThrowFinished;
    public event Action OnAimCancelled;

    public State CurrentState { get; private set; } = State.Ready;
    public bool InputEnabled = true;           // 程式 B 在非捕魚模式時設為 false

    [Header("除錯（測試完可關掉）")]
    public bool verboseLog = true;             // 在 Console 印出每個步驟
    public Key debugThrowKey = Key.F;          // 按這個鍵直接往前丟（不用甩滑鼠）
    public float debugThrowSpeed = 6f;

    readonly Queue<(float t, Vector3 pos)> samples = new();
    Projectile heldProjectile;
    bool waitTriggerRelease;                   // 取消後要先放開扳機才能再瞄準

    void OnEnable()  { triggerAction.action.Enable(); gripAction.action.Enable(); }
    void OnDisable() { triggerAction.action.Disable(); gripAction.action.Disable(); }

    void Update()
    {
        if (!InputEnabled) return;
        float trigger = triggerAction.action.ReadValue<float>();

        if (verboseLog && gripAction.action.WasPressedThisFrame())
            Log($"偵測到 Grip（目前狀態：{CurrentState}）");

        // 除錯丟擲：按 F 用固定速度往視線前方丟
        if (CurrentState == State.Ready && Keyboard.current != null
            && Keyboard.current[debugThrowKey].wasPressedThisFrame)
        {
            BeginAim();
            Vector3 dir = Camera.main ? Camera.main.transform.forward : holdPoint.forward;
            Launch(dir * debugThrowSpeed, "除錯鍵");
            return;
        }

        switch (CurrentState)
        {
            case State.Ready:
                if (waitTriggerRelease)
                {
                    if (trigger < triggerReleaseThreshold) waitTriggerRelease = false;
                }
                else if (trigger > triggerPressThreshold) BeginAim();
                break;

            case State.Aiming:
                RecordSample();
                if (gripAction.action.WasPressedThisFrame()) { CancelAim(); waitTriggerRelease = true; }
                else if (trigger < triggerReleaseThreshold) Release();
                break;
        }
    }

    void BeginAim()
    {
        samples.Clear();
        var go = Instantiate(currentTool.projectilePrefab, holdPoint.position, holdPoint.rotation, holdPoint);
        heldProjectile = go.GetComponent<Projectile>();
        heldProjectile.SetHeld(true);
        CurrentState = State.Aiming;
        Log("開始瞄準，魚叉已生成");
    }

    void RecordSample()
    {
        float now = Time.time;
        samples.Enqueue((now, holdPoint.position)); // 世界座標
        while (samples.Count > 2 && samples.Peek().t < now - sampleWindow) samples.Dequeue();
    }

    Vector3 GetAverageVelocity()
    {
        if (samples.Count < 2) return Vector3.zero;
        var first = samples.Peek();
        (float t, Vector3 pos) last = default;
        foreach (var s in samples) last = s;
        float dt = last.t - first.t;
        return dt > 0f ? (last.pos - first.pos) / dt : Vector3.zero;
    }

    void Release()
    {
        Vector3 v = GetAverageVelocity();
        Log($"放開扳機，手速 {v.magnitude:F2} m/s（門檻 {minThrowSpeed}）");
        if (v.magnitude < minThrowSpeed) { Log("手速太慢 → 視為取消"); CancelAim(); return; } // 誤觸保護

        Launch(v * currentTool.forceMultiplier, "投擲");
    }

    void Launch(Vector3 velocity, string source)
    {
        heldProjectile.transform.SetParent(null);
        heldProjectile.Launch(velocity, currentTool, this);
        heldProjectile = null;
        CurrentState = State.Flying;
        Log($"{source}：魚叉飛出，初速 {velocity.magnitude:F2} m/s");
    }

    void CancelAim()
    {
        if (heldProjectile) Destroy(heldProjectile.gameObject);
        heldProjectile = null;
        CurrentState = State.Ready;
        Log("取消瞄準");
        OnAimCancelled?.Invoke();
    }

    // Projectile 結算時呼叫：工具「自動回到手上」= 回到 Ready
    public void ReportResult(bool hit)
    {
        CurrentState = State.Ready;
        Log(hit ? "結算：命中" : "結算：未命中");
        OnThrowFinished?.Invoke(hit);
    }

    void Log(string msg)
    {
        if (verboseLog) Debug.Log($"[Throw] {msg}");
    }

    public void SetTool(ToolData tool)
    {
        if (CurrentState != State.Ready) return; // 瞄準/飛行中不能換
        currentTool = tool;
    }
}
