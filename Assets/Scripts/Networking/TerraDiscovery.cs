using System.Net;
using System;
using Mirror;
using Mirror.Discovery;
using UnityEngine.Events;

/*
	Discovery Guide: https://mirror-networking.com/docs/Guides/NetworkDiscovery.html
    Documentation: https://mirror-networking.com/docs/Components/NetworkDiscovery.html
    API Reference: https://mirror-networking.com/docs/api/Mirror.Discovery.NetworkDiscovery.html
*/

public struct DiscoveryRequest : NetworkMessage
    {
        // Add properties for whatever information you want sent by clients
        // in their broadcast messages that servers will consume
    }

    public struct DiscoveryResponse : NetworkMessage
    {
    // Add properties for whatever information you want the server to return to
    // clients for them to display or consume for establishing a connection.
    public long serverId;
    public int maxPlayers;
    public int nplayers;
    public string name;
    public string address;
    public bool banned;
    public string port;

    }

    [Serializable]
    public class DiscoveryFoundUnityEvent : UnityEvent<DiscoveryResponse> { };


    public class TerraDiscovery : NetworkDiscoveryBase<DiscoveryRequest, DiscoveryResponse>
    {

    public DiscoveryFoundUnityEvent OnDiscoveryFound;
    #region Server

    /// <summary>
    /// Reply to the client to inform it of this server
    /// </summary>
    /// <remarks>
    /// Override if you wish to ignore server requests based on
    /// custom criteria such as language, full server game mode or difficulty
    /// </remarks>
    /// <param name="request">Request comming from client</param>
    /// <param name="endpoint">Address of the client that sent the request</param>
    protected override void ProcessClientRequest(DiscoveryRequest request, IPEndPoint endpoint)
        {
            base.ProcessClientRequest(request, endpoint);
        }

        /// <summary>
        /// Process the request from a client
        /// </summary>
        /// <remarks>
        /// Override if you wish to provide more information to the clients
        /// such as the name of the host player
        /// </remarks>
        /// <param name="request">Request comming from client</param>
        /// <param name="endpoint">Address of the client that sent the request</param>
        /// <returns>A message containing information about this server</returns>
        protected override DiscoveryResponse ProcessRequest(DiscoveryRequest request, IPEndPoint endpoint)
        {
        return new DiscoveryResponse()
        {
            maxPlayers = gameObject.GetComponent<TerraNetworkManager>().maxConnections,
            nplayers = gameObject.GetComponent<TerraNetworkManager>().jugadores.Count,
            name = gameObject.GetComponent<TerraNetworkManager>().nombreMundo,
            serverId = RandomLong(),
            address = "",
            banned = gameObject.GetComponent<TerraNetworkManager>().listanegra.ContainsValue("::ffff:"+endpoint.Address.ToString())

        };
        }

        #endregion

        #region Client

        /// <summary>
        /// Create a message that will be broadcasted on the network to discover servers
        /// </summary>
        /// <remarks>
        /// Override if you wish to include additional data in the discovery message
        /// such as desired game mode, language, difficulty, etc... </remarks>
        /// <returns>An instance of ServerRequest with data to be broadcasted</returns>
        protected override DiscoveryRequest GetRequest()
        {
            return new DiscoveryRequest();
        }

        /// <summary>
        /// Process the answer from a server
        /// </summary>
        /// <remarks>
        /// A client receives a reply from a server, this method processes the
        /// reply and raises an event
        /// </remarks>
        /// <param name="response">Response that came from the server</param>
        /// <param name="endpoint">Address of the server that replied</param>
        protected override void ProcessResponse(DiscoveryResponse response, IPEndPoint endpoint) 
        {
            response.port = endpoint.Port.ToString();
            response.address = endpoint.Address.ToString();
            OnDiscoveryFound.Invoke(response);
        }

        #endregion
    }
