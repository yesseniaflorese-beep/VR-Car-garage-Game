using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class GameFlowManager : MonoBehaviour
{
    public static bool Playing = true;

    public string[] requiredIds = { "Battery", "Wheels", "Door" };
    public float timeLimit = 120f;        // tiempo total
    public float threeStarsTime = 40f;    // 3 estrellas si terminas en este tiempo o menos
    public float twoStarsTime = 80f;      // 2 estrellas hasta aquí; después, 1
    public TMP_Text timeText, progressText, resultText;

    float timeLeft;
    int errors;
    readonly HashSet<string> done = new HashSet<string>();

    float Used => timeLimit - timeLeft;   // incluye lo que restaron los errores

    void OnEnable()
    {
        GameEvents.OnPenalty += AddPenalty;
        GameEvents.OnMinigameCompleted += OnCompleted;
    }

    void OnDisable()
    {
        GameEvents.OnPenalty -= AddPenalty;
        GameEvents.OnMinigameCompleted -= OnCompleted;
    }

    void Start()
    {
        Playing = true;
        timeLeft = timeLimit;
        errors = 0;
        done.Clear();
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

    void AddPenalty(float seconds)
    {
        if (!Playing) return;
        errors++;
        timeLeft -= seconds;
        if (timeLeft <= 0f) Lose();
        UpdateUI();
    }

    void OnCompleted(string id)
    {
        if (!Playing || System.Array.IndexOf(requiredIds, id) < 0) return;
        if (!done.Add(id)) return;

        Debug.Log($"{id} terminado. Tiempo usado: {Used:0.0} s");
        if (done.Count >= requiredIds.Length) Win();
        UpdateUI();
    }

    void Win()
    {
        Playing = false;
        int stars = Used <= threeStarsTime ? 3 : Used <= twoStarsTime ? 2 : 1;
        resultText.color = Color.green;
        resultText.text = $"¡Todo listo!  {stars} estrellas\nTiempo usado: {Format(Used)}  |  Errores: {errors}";
        Debug.Log($"Victoria: {stars} estrellas");
        UpdateUI();
    }

    void Lose()
    {
        Playing = false;
        timeLeft = 0f;
        resultText.color = Color.red;
        resultText.text = "¡Se acabó el tiempo!\n0 estrellas";
        Debug.Log("Derrota: 0 estrellas");
    }

    void UpdateUI()
    {
        timeText.text = $"Tiempo: {Format(timeLeft)}";
        timeText.color = timeLeft < 15f ? Color.red : Color.white;
        progressText.text = $"Minijuegos: {done.Count} / {requiredIds.Length}";
    }

    static string Format(float t)
    {
        int s = Mathf.CeilToInt(Mathf.Max(0f, t));
        return $"{s / 60:00}:{s % 60:00}";
    }
}