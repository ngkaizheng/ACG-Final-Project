using UnityEngine;
using Fusion;

[CreateAssetMenu(menuName = "Events/Lobby Player List Data Event")]
public class LobbyPlayerListDataEvent : GameEvent<NetworkLinkedList<LobbyPlayerData>> { }