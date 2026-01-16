# Local-Only Multiplayer Implementation

## Overview
This implementation adds support for local-only multiplayer sessions (NetworkSessionType.Local and NetworkSessionType.LocalWithLeaderboards) without requiring Steam networking.

## Changes Made

### 1. NetworkEvent Structure Enhancement
**File:** `src/Net/NetworkSession.cs`

Added a `Sender` field to the `NetworkEvent` struct to track which gamer sent a packet:
```csharp
internal struct NetworkEvent
{
    public NetworkEventType Type;
    public NetworkGamer Gamer;
    public NetworkGamer Sender;  // NEW: Track the sender of the packet
    public byte[] Packet;
    public SendDataOptions Reliable;
    public NetworkSessionState State;
    public NetworkSessionEndReason Reason;
}
```

### 2. SendData Methods Update
**File:** `src/Net/LocalNetworkGamer.cs`

Updated all four `SendData` method overloads to populate the `Sender` field when creating PacketSend events:
- `SendData(byte[] data, SendDataOptions options)`
- `SendData(byte[] data, int offset, int count, SendDataOptions options)`
- `SendData(byte[] data, SendDataOptions options, NetworkGamer recipient)`
- `SendData(byte[] data, int offset, int count, SendDataOptions options, NetworkGamer recipient)`
- `SendData(PacketWriter data, SendDataOptions options)`
- `SendData(PacketWriter data, SendDataOptions options, NetworkGamer recipient)`

Each now includes:
```csharp
Sender = this,  // NEW: Set the sender to the current LocalNetworkGamer
```

### 3. Local Session Detection
**File:** `src/Net/NetworkSession.cs`

Added a helper method to determine if the session is local-only:
```csharp
private bool IsLocalSession()
{
    return SessionType == NetworkSessionType.Local ||
           SessionType == NetworkSessionType.LocalWithLeaderboards;
}
```

### 4. Packet Sending Logic
**File:** `src/Net/NetworkSession.cs` - `Update()` method

Modified the PacketSend event handling to route packets differently based on session type:

**For Local Sessions:**
- Packets are delivered directly to the recipient's packet queue
- No Steam P2P networking is used
- Sender does not receive their own packets
- Implementation:
  ```csharp
  if (IsLocalSession())
  {
      // Only deliver to local gamers (sender should not receive their own packet)
      if (evt.Gamer.IsLocal && evt.Gamer != evt.Sender)
      {
          LocalNetworkGamer localRecipient = evt.Gamer as LocalNetworkGamer;
          if (localRecipient != null)
          {
              NetworkEvent receiveEvt = new NetworkEvent()
              {
                  Gamer = evt.Sender,
                  Packet = evt.Packet
              };
              localRecipient.packetQueue.Enqueue(receiveEvt);
          }
      }
  }
  ```

**For Networked Sessions:**
- Uses the existing Steam P2P networking
- No changes to existing behavior

### 5. Packet Receiving Logic
**File:** `src/Net/NetworkSession.cs` - `Update()` method

Added a guard to skip Steam P2P packet reading for local sessions:
```csharp
// Only read from Steam P2P for networked sessions, not for local sessions
if (!IsLocalSession())
{
    // ... existing Steam P2P packet reading code ...
}
```

## How It Works

### Sending Flow (Local Session)
1. Developer calls `localGamer.SendData(data, options)`
2. `SendData` creates a PacketSend event for each gamer in the session
   - `evt.Gamer` = recipient
   - `evt.Sender` = sender (the LocalNetworkGamer)
   - `evt.Packet` = data to send
3. Event is queued in `networkEvents`
4. On `session.Update()`:
   - Event is dequeued
   - `IsLocalSession()` returns true
   - Packet is directly enqueued to recipient's `packetQueue`
   - Sender is excluded (doesn't receive their own packet)

### Receiving Flow (Local Session)
1. Developer calls `localGamer.ReceiveData(data, out sender)`
2. Packet is dequeued from the gamer's `packetQueue`
3. `sender` is set to the original sender (from `evt.Gamer`)
4. Data is returned to the caller

## Benefits
- ✅ Same API for both local and networked multiplayer
- ✅ No Steam networking overhead for local-only games
- ✅ Minimal code changes (surgical modifications)
- ✅ No breaking changes to existing functionality
- ✅ Clean separation between local and networked sessions

## Testing
To test local-only multiplayer:

1. Create a local session:
   ```csharp
   NetworkSession session = NetworkSession.Create(
       NetworkSessionType.Local,
       2,  // maxLocalGamers
       4   // maxGamers
   );
   ```

2. Send data from one gamer:
   ```csharp
   LocalNetworkGamer gamer1 = session.LocalGamers[0];
   byte[] data = System.Text.Encoding.UTF8.GetBytes("Hello!");
   gamer1.SendData(data, SendDataOptions.Reliable);
   ```

3. Update the session:
   ```csharp
   session.Update();
   ```

4. Receive data on another gamer:
   ```csharp
   LocalNetworkGamer gamer2 = session.LocalGamers[1];
   if (gamer2.IsDataAvailable)
   {
       byte[] buffer = new byte[1024];
       NetworkGamer sender;
       int bytesRead = gamer2.ReceiveData(buffer, out sender);
       // sender will be gamer1
   }
   ```

## Compatibility
- Works with both `NetworkSessionType.Local` and `NetworkSessionType.LocalWithLeaderboards`
- All other session types continue to use Steam P2P networking
- No changes required to existing code using networked sessions
