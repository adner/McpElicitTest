# Data Migration in Dataverse using MCP Servers

This is the code for a demo that I posted to [LinkedIn](https://www.linkedin.com/posts/andreas-adner-70b1153_data-migration-in-dataverse-using-ai-activity-7353181675943530496-rQ9z?utm_source=share&utm_medium=member_desktop&rcm=ACoAAACM8rsBEgQIrYgb4NZAbnxwfDRk_Tu5e3w) and that shows how an LLM can be used for data migration in Dataverse. A video showing this can be found [here](https://youtu.be/mtMmYbqpJW0).

This repo contains the code for an MCP Server that migrates data that is provided to the server in the format produced by the [Dataverse MCP Server](https://learn.microsoft.com/en-us/power-apps/maker/data-platform/data-platform-mcp), as shown in the demo.

The MCP Server relies on the MCP Client feature *[elicitation](https://modelcontextprotocol.io/specification/2025-06-18/client/elicitation)* - a feature that is currently (as of 2025-07-23) not commonly implemented in MCP Clients. Visual Studio Code with Github Copilot [supports](https://code.visualstudio.com/docs/copilot/chat/mcp-servers#_mcp-elicitations) it, so VS Code was my MCP Client of choice for this demo. 

The [Model Context Protocol C# SDK](https://github.com/modelcontextprotocol/csharp-sdk) was used to implement the MCP Server.