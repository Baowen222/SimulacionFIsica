
using UnityEngine;

public class JugadorAgente : MonoBehaviour
{
    public enum Equipo { Azul, Rojo }
    public Equipo equipo;

    public enum EstadoJugador
    {
        Poseer,
        Apoyar,
        Presionar,
        Cubrir
    }

    public EstadoJugador estadoActual;

    public Transform balon;
    public float velocidad = 2.5f;
    public float radioPercepcion = 10f;

    private Vector3 posicionInicial;
    private ControlBalon controlBalon;
    private JugadorAgente[] jugadores;

    void Start()
    {
        posicionInicial = transform.position;

        if (balon != null)
            controlBalon = balon.GetComponent<ControlBalon>();

        jugadores = FindObjectsByType<JugadorAgente>(
            FindObjectsSortMode.None
        );
    }

    void Update()
    {
        if (controlBalon == null ||
            controlBalon.poseedor == null)
            return;

        Transform poseedor = controlBalon.poseedor;

        JugadorAgente agentePoseedor =
            poseedor.GetComponent<JugadorAgente>();

        if (agentePoseedor == null) return;

        float direccion = equipo == Equipo.Azul ? 1f : -1f;

        // Jugador que tiene el balón
        if (poseedor == transform)
        {
            estadoActual = EstadoJugador.Poseer;

            Vector3 destino = new Vector3(
                direccion * 6f,
                transform.position.y,
                transform.position.z
            );

            Mover(destino);
            return;
        }

        // Compañeros del poseedor
        if (agentePoseedor.equipo == equipo)
        {
            estadoActual = EstadoJugador.Apoyar;

            float lateral = posicionInicial.y >= 0 ? 1.5f : -1.5f;

            Vector3 destino = new Vector3(
                Mathf.Clamp(
                    poseedor.position.x + direccion * 1.5f,
                    -7f, 7f
                ),
                lateral,
                transform.position.z
            );

            Mover(destino);
            return;
        }

        // Equipo defensor
        float distancia = Vector2.Distance(
            transform.position,
            balon.position
        );

        if (distancia <= radioPercepcion &&
            EsDefensorMasCercano())
        {
            estadoActual = EstadoJugador.Presionar;

            if (distancia > 0.5f)
                Mover(balon.position);
        }
        else
        {
            estadoActual = EstadoJugador.Cubrir;

            Vector3 destino = new Vector3(
                Mathf.Clamp(
                    posicionInicial.x +
                    Mathf.Clamp(
                        (poseedor.position.x - posicionInicial.x) * 0.25f,
                        -1.2f, 1.2f
                    ),
                    -7f, 7f
                ),
                Mathf.Lerp(
                    posicionInicial.y,
                    poseedor.position.y,
                    0.15f
                ),
                transform.position.z
            );

            Mover(destino);
        }
    }

    bool EsDefensorMasCercano()
    {
        float miDistancia = Vector2.Distance(
            transform.position, balon.position
        );

        foreach (JugadorAgente jugador in jugadores)
        {
            if (jugador == this || jugador.equipo != equipo)
                continue;

            float otraDistancia = Vector2.Distance(
                jugador.transform.position, balon.position
            );

            if (otraDistancia < miDistancia - 0.001f ||
                (Mathf.Abs(otraDistancia - miDistancia) <= 0.001f &&
                 jugador.GetInstanceID() < GetInstanceID()))
                return false;
        }

        return true;
    }

    void Mover(Vector3 destino)
    {
        destino.z = transform.position.z;

        transform.position = Vector3.MoveTowards(
            transform.position,
            destino,
            velocidad * Time.deltaTime
        );
    }
}
