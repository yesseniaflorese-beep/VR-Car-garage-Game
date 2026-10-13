using TMPro;
using UnityEngine;
using UnityEngine.XR.Interaction.Toolkit.Interactables;

public class GameFlowManager : MonoBehaviour
{
    public static bool Playing = true;

    public string minigameId = "Battery";
    public float timeLimit = 120f;        // tiempo total
    public float threeStarsTime = 40f;    // 3 estrellas si terminas en este tiempo o menos
    public float twoStarsTime = 80f;      // 2 estrellas hasta aquí; después, 1
    public int pointsPerConnection = 100;
    public int pointsPerError = 50;
    public int pointsPerSecondLeft = 10;
    public TMP_Text timeText, scoreText, resultText;

    float timeLeft;
    int score, errors, lastCount;

    float Used => timeLimit - timeLeft;   // incluye lo que restaron los errores
    int Score => Mathf.Max(0, score);

    void OnEnable()
    {
        GameEvents.OnPenalty += AddPenalty;
        GameEvents.OnMinigameCompleted += OnCompleted;
        TerminalLogic.OnConnectionsChanged += OnConnections;
    }

    void OnDisable()
    {
        GameEvents.OnPenalty -= AddPenalty;
        GameEvents.OnMinigameCompleted -= OnCompleted;
        TerminalLogic.OnConnectionsChanged -= OnConnections;
    }

    void Start()
    {
        Playing = true;
        timeLeft = timeLimit;
        score = errors = lastCount = 0;
        resultText.text = "";
        UpdateUI();
    }

    void Update()
    {
        if (!Playing) return;
        timeLeft -= Time.deltaTime;
        if (timeLeft <= 0f) Lose();
        UpdateUI();
    }

    void OnConnections(int n)
    {
        if (!Playing) return;
        score += (n - lastCount) * pointsPerConnection;
        lastCount = n;
        UpdateUI();
    }

    void AddPenalty(float seconds)
    {
        if (!Playing) return;
        errors++;
        score -= pointsPerError;
        timeLeft -= seconds;
        if (timeLeft <= 0f) Lose();
        UpdateUI();
    }

    void OnCompleted(string id)
    {
        if (Playing && id == minigameId) Win();
    }

    void Win()
    {
        Playing = false;
        score += Mathf.RoundToInt(timeLeft) * pointsPerSecondLeft;
        int stars = Used <= threeStarsTime ? 3 : Used <= twoStarsTime ? 2 : 1;

        resultText.color = Color.green;
        resultText.text = $"¡Completado!  {stars} estrellas\nPuntaje: {Score}\nTiempo: {Format(Used)}  |  Errores: {errors}";
        LockInteractions();
        UpdateUI();
        Debug.Log($"Victoria: {stars} estrellas, {Score} puntos");
    }

    void Lose()
    {
        if (!Playing) return;
        Playing = false;
        timeLeft = 0f;

        resultText.color = Color.red;
        resultText.text = $"¡Se acabó el tiempo!  0 estrellas\nPuntaje: {Score}\nErrores: {errors}";
        LockInteractions();
        UpdateUI();
        Debug.Log($"Derrota: {Score} puntos");
    }

    // Al terminar, nada más se puede agarrar
    void LockInteractions()
    {
        foreach (var g in FindObjectsByType<XRGrabInteractable>(FindObjectsSortMode.None))
            g.enabled = false;
    }

    void UpdateUI()
    {
        timeText.text = $"Tiempo: {Format(timeLeft)}";
        timeText.color = timeLeft < 15f ? Color.red : Color.white;
        scoreText.text = $"Puntos: {Score}";
    }

    static string Format(float t)
    {
        int s = Mathf.CeilToInt(Mathf.Max(0f, t));
        return $"{s / 60:00}:{s % 60:00}";
    }
}