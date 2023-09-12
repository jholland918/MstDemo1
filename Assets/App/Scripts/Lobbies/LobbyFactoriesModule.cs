using MasterServerToolkit.MasterServer;
using UnityEngine;

namespace Assets.App.Scripts.Lobbies
{
    /// <summary>
    /// Adds <see cref="ILobbyFactory"/> implementations to the <see cref="LobbiesModule"/>
    /// </summary>
    /// <remarks>Should be attached to the same GameObject as the <see cref="LobbiesModule"/></remarks>
    internal class LobbyFactoriesModule : MonoBehaviour
    {
        private LobbiesModule _lobbiesModule;

        protected void Awake()
        {
            _lobbiesModule = GetComponent<LobbiesModule>();
        }

        protected void Start()
        {
            _lobbiesModule.AddFactory(new OneVsOneLobbyFactory(_lobbiesModule));
            _lobbiesModule.AddFactory(new TwoVsTwoLobbyFactory(_lobbiesModule));
            _lobbiesModule.AddFactory(new SurvivalLobbyFactory(_lobbiesModule));
        }
    }
}
