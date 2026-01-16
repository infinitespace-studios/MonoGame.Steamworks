using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.GamerServices;
using Microsoft.Xna.Framework.Net;
using System;

namespace example;

/// <summary>
/// Simple test to demonstrate and verify local-only multiplayer support.
/// This test creates a local network session with multiple local gamers
/// and verifies that data sent from one gamer is received by others.
/// </summary>
public class LocalMultiplayerTest
{
    public static bool RunTest()
    {
        Console.WriteLine("Starting Local Multiplayer Test...");
        
        try
        {
            // Step 1: Create a local network session
            Console.WriteLine("Creating local network session with 2 local gamers...");
            NetworkSession session = NetworkSession.Create(
                NetworkSessionType.Local,
                2,  // maxLocalGamers
                4   // maxGamers
            );
            
            if (session.SessionType != NetworkSessionType.Local)
            {
                Console.WriteLine("ERROR: Session type is not Local!");
                session.Dispose();
                return false;
            }
            
            Console.WriteLine($"Session created. Local gamers count: {session.LocalGamers.Count}");
            
            // Step 2: Add a second local gamer (if only one exists)
            if (session.LocalGamers.Count < 2 && Gamer.SignedInGamers.Count >= 2)
            {
                session.AddLocalGamer(Gamer.SignedInGamers[1]);
                Console.WriteLine($"Added second gamer. Local gamers count: {session.LocalGamers.Count}");
            }
            
            if (session.LocalGamers.Count < 2)
            {
                Console.WriteLine("WARNING: Need at least 2 local gamers for full test. Test will be limited.");
            }
            
            // Step 3: Start the game session
            if (session.IsHost)
            {
                session.StartGame();
                Console.WriteLine("Game session started.");
            }
            
            // Step 4: Send data from first gamer
            LocalNetworkGamer gamer1 = session.LocalGamers[0];
            byte[] testData = System.Text.Encoding.UTF8.GetBytes("Hello from gamer 1!");
            
            Console.WriteLine($"Gamer 1 sending data: '{System.Text.Encoding.UTF8.GetString(testData)}'");
            gamer1.SendData(testData, SendDataOptions.Reliable);
            
            // Step 5: Update the session to process packets
            session.Update();
            
            // Step 6: Check if data was received by other gamers
            if (session.LocalGamers.Count >= 2)
            {
                LocalNetworkGamer gamer2 = session.LocalGamers[1];
                
                Console.WriteLine($"Gamer 2 checking for data. IsDataAvailable: {gamer2.IsDataAvailable}");
                
                if (gamer2.IsDataAvailable)
                {
                    byte[] receivedData = new byte[1024];
                    NetworkGamer sender;
                    int bytesRead = gamer2.ReceiveData(receivedData, out sender);
                    
                    string receivedMessage = System.Text.Encoding.UTF8.GetString(receivedData, 0, bytesRead);
                    Console.WriteLine($"Gamer 2 received {bytesRead} bytes from {sender.Gamertag}: '{receivedMessage}'");
                    
                    if (receivedMessage == "Hello from gamer 1!" && sender == gamer1)
                    {
                        Console.WriteLine("SUCCESS: Data was correctly delivered from gamer 1 to gamer 2!");
                    }
                    else
                    {
                        Console.WriteLine("ERROR: Received data does not match or sender is incorrect!");
                        session.Dispose();
                        return false;
                    }
                }
                else
                {
                    Console.WriteLine("ERROR: Gamer 2 did not receive data!");
                    session.Dispose();
                    return false;
                }
                
                // Step 7: Verify that gamer 1 did not receive their own packet
                Console.WriteLine($"Gamer 1 checking for data (should not have any). IsDataAvailable: {gamer1.IsDataAvailable}");
                if (gamer1.IsDataAvailable)
                {
                    Console.WriteLine("ERROR: Gamer 1 received their own packet (should not happen)!");
                    session.Dispose();
                    return false;
                }
            }
            
            // Cleanup
            session.Dispose();
            Console.WriteLine("Test completed successfully!");
            return true;
        }
        catch (Exception ex)
        {
            Console.WriteLine($"ERROR: Test failed with exception: {ex.Message}");
            Console.WriteLine($"Stack trace: {ex.StackTrace}");
            return false;
        }
    }
}
