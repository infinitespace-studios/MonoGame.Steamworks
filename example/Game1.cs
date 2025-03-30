using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using Microsoft.Xna.Framework.GamerServices;
using Microsoft.Xna.Framework.Net;
using System;
using System.Collections.Generic;

namespace example;

public class Game1 : Game
{
    private GraphicsDeviceManager _graphics;
    private SpriteBatch _spriteBatch;
    private Texture2D _playerTexture;
    private Dictionary<int, Vector2> _playerPositions;
    private int _playerIndex;
    private NetworkSession _networkSession;
    private PacketReader _packetReader;
    private PacketWriter _packetWriter;
    private bool _isHost;

    public Game1()
    {
        _graphics = new GraphicsDeviceManager(this);
        Content.RootDirectory = "Content";
        Components.Add(new GamerServicesComponent(this)); // Required for XNA networking
    }

    protected override void Initialize()
    {
        // Ensure the player is signed in to Xbox Live
        if (!Guide.IsVisible && Gamer.SignedInGamers.Count == 0)
        {
            Guide.ShowSignIn(1, false);
        }

        _playerPositions = new Dictionary<int, Vector2>();
        _packetReader = new PacketReader();
        _packetWriter = new PacketWriter();
        base.Initialize();
    }

    protected override void LoadContent()
    {
        _spriteBatch = new SpriteBatch(GraphicsDevice);
        _playerTexture = new Texture2D(GraphicsDevice, 20, 20); // Placeholder texture

        Color[] colorData = new Color[20 * 20];
        for (int i = 0; i < colorData.Length; i++)
            colorData[i] = Color.Red;

        _playerTexture.SetData(colorData);
    }

    protected override void Update(GameTime gameTime)
    {
        if (_networkSession == null)
        {
            HandleNetworkSession();
        }
        else
        {
            HandleNetworkGameplay();
        }

        base.Update(gameTime);
    }

    private void HandleNetworkSession()
    {
        if (Gamer.SignedInGamers.Count == 0)
        {
            // Ensure the player is signed in before proceeding
            if (!Guide.IsVisible)
            {
                Guide.ShowSignIn(1, false);
            }
            return;
        }

        if (Keyboard.GetState().IsKeyDown(Keys.C))
        {
            _networkSession = NetworkSession.Create(NetworkSessionType.SystemLink, 4, 4);
            _networkSession.SessionEnded += NetworkSessionEnded;
            _isHost = true;
            _playerIndex = 0;
            _playerPositions[_playerIndex] = new Vector2(100, 100);
        }
        else if (Keyboard.GetState().IsKeyDown(Keys.J))
        {
            AvailableNetworkSessionCollection availableSessions = NetworkSession.Find(NetworkSessionType.SystemLink, 4, null);
            if (availableSessions.Count > 0)
            {
                _networkSession = NetworkSession.Join(availableSessions[0]);
                _networkSession.SessionEnded += NetworkSessionEnded;
                _isHost = false;
            }
        }
    }

    private void HandleNetworkGameplay()
    {
        if (_networkSession == null) return;

        _networkSession.Update();

        if (_networkSession.IsHost && _networkSession.RemoteGamers.Count > 0)
        {
            AssignPlayerIndexes();
        }

        foreach (LocalNetworkGamer gamer in _networkSession.LocalGamers)
        {
            HandlePlayerInput(gamer);
            SendPlayerData(gamer);
        }

        ReceiveNetworkData();
    }

    private void AssignPlayerIndexes()
    {
        int index = 1; // Host is always index 0
        foreach (NetworkGamer gamer in _networkSession.RemoteGamers)
        {
            if (!_playerPositions.ContainsKey(index))
            {
                _playerPositions[index] = new Vector2(200 + (index * 50), 200);
                index++;
            }
        }
    }

    private void HandlePlayerInput(LocalNetworkGamer gamer)
    {
        KeyboardState state = Keyboard.GetState();

        if (!_playerPositions.ContainsKey(_playerIndex))
            _playerPositions[_playerIndex] = new Vector2(100, 100);

        Vector2 movement = Vector2.Zero;
        if (state.IsKeyDown(Keys.W)) movement.Y -= 3;
        if (state.IsKeyDown(Keys.S)) movement.Y += 3;
        if (state.IsKeyDown(Keys.A)) movement.X -= 3;
        if (state.IsKeyDown(Keys.D)) movement.X += 3;

        _playerPositions[_playerIndex] += movement;
    }

    private void SendPlayerData(LocalNetworkGamer gamer)
    {
        _packetWriter.Write(_playerIndex);
        _packetWriter.Write(_playerPositions[_playerIndex].X);
        _packetWriter.Write(_playerPositions[_playerIndex].Y);
        gamer.SendData(_packetWriter, SendDataOptions.InOrder);
    }

    private void ReceiveNetworkData()
    {
        foreach (NetworkGamer gamer in _networkSession.AllGamers)
        {
            if (!gamer.IsLocal) continue;

            LocalNetworkGamer localGamer = gamer as LocalNetworkGamer;

            while (localGamer.IsDataAvailable)
            {
                localGamer.ReceiveData(_packetReader, out NetworkGamer sender);
                int index = _packetReader.ReadInt32();
                float x = _packetReader.ReadSingle();
                float y = _packetReader.ReadSingle();
                _playerPositions[index] = new Vector2(x, y);
            }
        }
    }

    private void NetworkSessionEnded(object sender, NetworkSessionEndedEventArgs e)
    {
        _networkSession.Dispose();
        _networkSession = null;
    }

    protected override void Draw(GameTime gameTime)
    {
        GraphicsDevice.Clear(Color.CornflowerBlue);
        _spriteBatch.Begin();

        foreach (var player in _playerPositions)
        {
            _spriteBatch.Draw(_playerTexture, player.Value, Color.White);
        }

        _spriteBatch.End();
        base.Draw(gameTime);
    }
}
