using Microsoft.Agents.AI;
using System;
using System.Collections.Generic;
using System.Text;

namespace FoundryToolkit.Shared
{
    public static class Utils
    {
        public static async Task RunChatLoopWithSession(AIAgent agent, AgentSession session)
        {
            while (true)
            {
                Console.Write("\n> ");
                string input = Console.ReadLine();

                if (input.Equals("/new", StringComparison.CurrentCultureIgnoreCase))
                {
                    session = await agent.CreateSessionAsync();
                    Console.Clear();
                    continue;
                }

                await foreach (var update in agent.RunStreamingAsync(input, session))
                    Console.Write(update);
            }
        }
    }
}
