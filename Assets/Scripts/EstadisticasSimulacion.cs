
using UnityEngine;
using TMPro;

public class EstadisticasSimulacion : MonoBehaviour
{
    [Header("Referencias")]
    public ControlBalon controlBalon;
    public TMP_Text textoEstadisticas;

    [Header("Experimento")]
    public float duracion = 60f;

    private float tiempo;
    private float posesionAzul;
    private float posesionRoja;
    private bool finalizada;

    private JugadorAgente[] jugadores;

    void Start()
    {
        jugadores = FindObjectsByType<JugadorAgente>(
            FindObjectsSortMode.None
        );

        ActualizarTexto();
    }

    void Update()
    {
        if (finalizada || controlBalon == null)
            return;

        float delta = Mathf.Min(
            Time.deltaTime,
            Mathf.Max(0f, duracion - tiempo)
        );

        tiempo += delta;

        if (controlBalon.poseedor != null)
        {
            JugadorAgente poseedor =
                controlBalon.poseedor.GetComponent<JugadorAgente>();

            if (poseedor != null)
            {
                if (poseedor.equipo == JugadorAgente.Equipo.Azul)
                    posesionAzul += delta;
                else
                    posesionRoja += delta;
            }
        }

        if (tiempo >= duracion)
        {
            FinalizarExperimento();
        }

        ActualizarTexto();
    }

    void FinalizarExperimento()
    {
        finalizada = true;

        // Detener los agentes.
        foreach (JugadorAgente jugador in jugadores)
        {
            if (jugador != null)
                jugador.enabled = false;
        }

        // Detener los pases y robos.
        if (controlBalon != null)
            controlBalon.enabled = false;

        Debug.Log("Experimento finalizado.");
    }

    void ActualizarTexto()
    {
        if (textoEstadisticas == null)
            return;

        float total = posesionAzul + posesionRoja;

        float porcentajeAzul =
            total > 0f ? posesionAzul / total * 100f : 0f;

        float porcentajeRojo =
            total > 0f ? posesionRoja / total * 100f : 0f;

        int pases = controlBalon != null
            ? controlBalon.pasesCompletados : 0;

        int recuperaciones = controlBalon != null
            ? controlBalon.recuperaciones : 0;

        textoEstadisticas.text =
            "Tiempo: " +
            Mathf.CeilToInt(Mathf.Max(0f, duracion - tiempo)) +
            " s\n" +
            "Posesión azul: " +
            porcentajeAzul.ToString("F1") + "%\n" +
            "Posesión roja: " +
            porcentajeRojo.ToString("F1") + "%\n" +
            "Pases completados: " + pases + "\n" +
            "Recuperaciones: " + recuperaciones;

        if (finalizada)
            textoEstadisticas.text +=
                "\nEXPERIMENTO FINALIZADO";
    }
}
