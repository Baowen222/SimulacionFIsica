
using UnityEngine;
using UnityEngine.UI;

public class GestorSimulacion : MonoBehaviour
{
    [Header("Controles")]
    public Slider sliderPresion;
    public Slider sliderVelocidadPase;
    public Slider sliderPercepcion;

    [Header("Simulacion")]
    public ControlBalon controlBalon;

    private JugadorAgente[] jugadores;

    void Start()
    {
        jugadores = FindObjectsByType<JugadorAgente>(
            FindObjectsSortMode.None
        );
    }

    void Update()
    {
        if (controlBalon == null ||
            controlBalon.poseedor == null)
            return;

        if (sliderPresion == null ||
            sliderVelocidadPase == null ||
            sliderPercepcion == null)
            return;

        JugadorAgente poseedor =
            controlBalon.poseedor.GetComponent<JugadorAgente>();

        if (poseedor == null) return;

        // Modificar velocidad de los defensores
        foreach (JugadorAgente jugador in jugadores)
        {
            if (jugador == null) continue;

            if (jugador.equipo != poseedor.equipo)
            {
                jugador.velocidad = sliderPresion.value;
            }
            else
            {
                jugador.velocidad = 2.5f;
            }

            // Modificar el radio de percepcion
            jugador.radioPercepcion = sliderPercepcion.value;
        }

        // Modificar la velocidad de los pases
        controlBalon.velocidadPase =
            sliderVelocidadPase.value;
    }
}
