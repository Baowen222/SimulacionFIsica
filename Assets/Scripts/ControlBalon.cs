
using System.Collections.Generic;
using UnityEngine;

public class ControlBalon : MonoBehaviour
{
    [Header("Posesion")]
    public Transform poseedor;
    public float distanciaBalon = 0.4f;
    public float distanciaRobo = 0.55f;
    public float distanciaRearme = 1.2f;
    public float tiempoProteccion = 1.5f;

    [Header("Pases")]
    public float distanciaPresion = 2f;
    public float velocidadPase = 7f;
    public float intervaloPases = 2f;
    public float distanciaIntercepcion = 0.3f;

    [Header("Estadisticas")]
    public int pasesCompletados;
    public int recuperaciones;

    private JugadorAgente[] jugadores;
    private readonly HashSet<JugadorAgente> defensoresHabilitados =
        new HashSet<JugadorAgente>();

    private Transform receptor;
    private bool enPase;

    private float siguienteRobo;
    private float siguientePase;

    void Start()
    {
        jugadores = FindObjectsByType<JugadorAgente>(
            FindObjectsSortMode.None
        );

        distanciaRearme = Mathf.Max(
            distanciaRearme,
            distanciaRobo + 0.1f
        );
    }

    void LateUpdate()
    {
        if (poseedor == null) return;

        if (enPase)
        {
            ActualizarPase();
            return;
        }

        JugadorAgente actual =
            poseedor.GetComponent<JugadorAgente>();

        if (actual == null) return;

        ColocarBalon(actual);

        ActualizarDefensores(actual);

        if (Time.time >= siguienteRobo)
        {
            if (ComprobarRobo(actual))
                return;
        }

        if (Time.time >= siguientePase)
            IntentarPase(actual);
    }

    void ActualizarDefensores(JugadorAgente actual)
    {
        foreach (JugadorAgente jugador in jugadores)
        {
            if (jugador == null ||
                jugador.equipo == actual.equipo)
                continue;

            float distancia = Vector2.Distance(
                jugador.transform.position,
                transform.position
            );

            // Solo habilitar un nuevo robo tras
            // haberse separado suficientemente.
            if (distancia >= distanciaRearme)
                defensoresHabilitados.Add(jugador);
        }
    }

    bool ComprobarRobo(JugadorAgente actual)
    {
        foreach (JugadorAgente jugador in jugadores)
        {
            if (jugador == null ||
                jugador.equipo == actual.equipo)
                continue;

            if (!defensoresHabilitados.Contains(jugador))
                continue;

            float distancia = Vector2.Distance(
                jugador.transform.position,
                transform.position
            );

            if (distancia <= distanciaRobo)
            {
                CambiarPosesion(jugador);

                Debug.Log("Robo: " + jugador.name);
                return true;
            }
        }

        return false;
    }

    void IntentarPase(JugadorAgente actual)
    {
        float distanciaRival = float.MaxValue;

        foreach (JugadorAgente jugador in jugadores)
        {
            if (jugador == null ||
                jugador.equipo == actual.equipo)
                continue;

            float distancia = Vector2.Distance(
                actual.transform.position,
                jugador.transform.position
            );

            distanciaRival = Mathf.Min(
                distanciaRival, distancia
            );
        }

        if (distanciaRival > distanciaPresion)
            return;

        JugadorAgente mejorCompanero = null;
        float mejorSeguridad = -1f;

        foreach (JugadorAgente candidato in jugadores)
        {
            if (candidato == null ||
                candidato == actual ||
                candidato.equipo != actual.equipo)
                continue;

            float distancia = Vector2.Distance(
                actual.transform.position,
                candidato.transform.position
            );

            if (distancia < 1f || distancia > 7f)
                continue;

            float seguridad = float.MaxValue;

            foreach (JugadorAgente rival in jugadores)
            {
                if (rival == null ||
                    rival.equipo == actual.equipo)
                    continue;

                float distanciaDefensor = Vector2.Distance(
                    candidato.transform.position,
                    rival.transform.position
                );

                seguridad = Mathf.Min(
                    seguridad, distanciaDefensor
                );
            }

            if (seguridad > mejorSeguridad)
            {
                mejorSeguridad = seguridad;
                mejorCompanero = candidato;
            }
        }

        if (mejorCompanero == null)
            return;

        receptor = mejorCompanero.transform;
        enPase = true;
        siguientePase = Time.time + intervaloPases;

        Debug.Log(
            "Pase: " + actual.name +
            " a " + mejorCompanero.name
        );
    }

    void ActualizarPase()
    {
        if (poseedor == null || receptor == null)
        {
            enPase = false;
            receptor = null;
            return;
        }

        transform.position = Vector3.MoveTowards(
            transform.position,
            receptor.position,
            velocidadPase * Time.deltaTime
        );

        JugadorAgente pasador =
            poseedor.GetComponent<JugadorAgente>();

        if (pasador == null)
        {
            enPase = false;
            receptor = null;
            return;
        }

        foreach (JugadorAgente jugador in jugadores)
        {
            if (jugador == null ||
                jugador.equipo == pasador.equipo)
                continue;

            float distancia = Vector2.Distance(
                jugador.transform.position,
                transform.position
            );

            if (distancia <= distanciaIntercepcion)
            {
                CambiarPosesion(jugador);

                Debug.Log(
                    "Intercepcion: " + jugador.name
                );
                return;
            }
        }

        if (Vector2.Distance(
            transform.position,
            receptor.position
        ) <= 0.12f)
        {
            poseedor = receptor;
            receptor = null;
            enPase = false;

            pasesCompletados++;

            siguienteRobo =
                Time.time + tiempoProteccion;

            JugadorAgente nuevoPoseedor =
                poseedor.GetComponent<JugadorAgente>();

            ColocarBalon(nuevoPoseedor);

            Debug.Log(
                "Pase completado: " +
                pasesCompletados
            );
        }
    }

    void CambiarPosesion(JugadorAgente jugador)
    {
        poseedor = jugador.transform;
        receptor = null;
        enPase = false;

        recuperaciones++;

        // Evitar nuevos robos sin separacion previa.
        defensoresHabilitados.Clear();

        siguienteRobo =
            Time.time + tiempoProteccion;

        siguientePase =
            Time.time + intervaloPases;

        ColocarBalon(jugador);
    }

    void ColocarBalon(JugadorAgente jugador)
    {
        if (jugador == null) return;

        float direccion =
            jugador.equipo == JugadorAgente.Equipo.Azul
            ? 1f : -1f;

        transform.position =
            poseedor.position +
            Vector3.right * distanciaBalon * direccion;
    }
}
