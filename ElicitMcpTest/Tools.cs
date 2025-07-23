using System.ComponentModel;
using Microsoft.PowerPlatform.Dataverse.Client;
using Microsoft.Xrm.Sdk;
using Microsoft.Xrm.Sdk.Messages;
using Microsoft.Xrm.Sdk.Metadata;
using ModelContextProtocol;
using ModelContextProtocol.Protocol;
using ModelContextProtocol.Server;

[McpServerToolType]
public sealed class DataverseCLITools
{
    private static string environmentUrl = string.Empty;

    [McpServerTool, Description("Tool for testing the Elicit command. If the parameters are not provided, set the missing parameters to 'NotSet'")]
    public static async Task<CallToolResult> TestElicit(IMcpServer server, string parameter1, string? parameter2)
    {

        var result = await server.ElicitAsync(
                new()
                {
                    Message = "Please provide more information.",
                    RequestedSchema = new()
                    {
                        Properties = new Dictionary<string, ElicitRequestParams.PrimitiveSchemaDefinition>()
                        {
                            ["prop1"] = new ElicitRequestParams.StringSchema
                            {
                                Title = "title1",
                                MinLength = 1,
                                MaxLength = 100,
                            },
                            ["prop2"] = new ElicitRequestParams.NumberSchema
                            {
                                Description = "description2",
                                Minimum = 0,
                                Maximum = 1000,
                            },
                            ["prop3"] = new ElicitRequestParams.BooleanSchema
                            {
                                Title = "title3",
                                Description = "description4",
                                Default = true,
                            },
                            ["prop4"] = new ElicitRequestParams.EnumSchema
                            {
                                Enum = ["option1", "option2", "option3"],
                                EnumNames = ["Name1", "Name2", "Name3"],
                            },
                        },
                    },
                },
                CancellationToken.None);

        return new CallToolResult
        {
            Content = [new TextContentBlock { Text = "success" }],
        };
    }

    [McpServerTool, Description("Migrates data to a Dataverse environment.")]
    public static async Task<string> MigrateData(IMcpServer server, RequestContext<CallToolRequestParams> context, [Description("The name of the table")] string tableName, [Description("A JSON array of items that are to be migrated, where each item contains a list of attributes in the following format {'[logicalname]:[value]'}")] string recordList)
    {
        List<Guid> skipped = new List<Guid>();
        List<Guid> overwritten = new List<Guid>();

        if (environmentUrl == string.Empty)
        {
            var result = await server.ElicitAsync(
                                 new()
                                 {
                                     Message = "Please add the URL of the Dataverse environment you want to migrate to.",
                                     RequestedSchema = new()
                                     {
                                         Properties = new Dictionary<string, ElicitRequestParams.PrimitiveSchemaDefinition>()
                                         {
                                             ["environmentUrl"] = new ElicitRequestParams.StringSchema
                                             {
                                                 Title = "Environment URL",
                                                 MinLength = 1,
                                                 MaxLength = 100,
                                             }
                                         },
                                     },
                                 },
                                 CancellationToken.None);

            if (result.Content != null && result.Content.TryGetValue("environmentUrl", out var urlValue))
            {
                environmentUrl = urlValue.ToString();
            }
        }

        string connectionString = $@"
    AuthType = ClientSecret;
    Url = {environmentUrl};
    ClientId = 448d2e...6b11e8;
    Secret = ~ER...aIm";

        var serviceClient = new ServiceClient(connectionString);

        // Safely access the progress token
        ProgressToken? progressToken = null;
        if (context.Params?.ProgressToken != null)
        {
            progressToken = (ProgressToken)context.Params.ProgressToken;
        }

        try
        {
            // Parse the JSON array of records
            var records = Newtonsoft.Json.JsonConvert.DeserializeObject<List<Dictionary<string, string>>>(recordList);

            if (records == null || records.Count == 0)
            {
                return "No records to migrate. Please provide a valid JSON array of records.";
            }

            // Total number of records to process
            int totalRecords = records.Count;

            // Log initial progress if we have a progress token
            if (progressToken != null)
            {
                await server.NotifyProgressAsync(progressToken.Value, new()
                {
                    Progress = 0,
                    Message = $"Starting migration of {totalRecords} records to {tableName} table",
                    Total = totalRecords
                });
            }

            var recordId = Guid.Empty;

            // Process each record
            for (int i = 0; i < totalRecords; i++)
            {
                var record = records[i];
                string recordDetails = tableName + " with ID " + recordId;

                Entity entity = new Entity(tableName);

                foreach (var attribute in record)
                {
                    if (attribute.Key == tableName + "id") recordId = new Guid(attribute.Value);

                    // Get metadata for the attribute to determine its data type
                    var retrieveAttributeRequest = new Microsoft.Xrm.Sdk.Messages.RetrieveAttributeRequest
                    {
                        EntityLogicalName = tableName,
                        LogicalName = attribute.Key
                    };

                    try
                    {
                        var response = (Microsoft.Xrm.Sdk.Messages.RetrieveAttributeResponse)serviceClient.Execute(retrieveAttributeRequest);
                        var attributeMetadata = response.AttributeMetadata;

                        // Process the value based on the attribute type
                        object typedValue;
                        switch (attributeMetadata.AttributeType)
                        {
                            case Microsoft.Xrm.Sdk.Metadata.AttributeTypeCode.Boolean:
                                typedValue = bool.Parse(attribute.Value);
                                break;
                            case Microsoft.Xrm.Sdk.Metadata.AttributeTypeCode.DateTime:
                                typedValue = DateTime.Parse(attribute.Value);
                                break;
                            case Microsoft.Xrm.Sdk.Metadata.AttributeTypeCode.Decimal:
                                typedValue = decimal.Parse(attribute.Value);
                                break;
                            case Microsoft.Xrm.Sdk.Metadata.AttributeTypeCode.Double:
                                typedValue = double.Parse(attribute.Value);
                                break;
                            case Microsoft.Xrm.Sdk.Metadata.AttributeTypeCode.Integer:
                                typedValue = int.Parse(attribute.Value);
                                break;
                            case Microsoft.Xrm.Sdk.Metadata.AttributeTypeCode.Money:
                                typedValue = new Money(decimal.Parse(attribute.Value));
                                break;
                            case Microsoft.Xrm.Sdk.Metadata.AttributeTypeCode.Picklist:
                                var optionSetValue = new OptionSetValue(int.Parse(attribute.Value));
                                typedValue = optionSetValue;
                                break;
                            case Microsoft.Xrm.Sdk.Metadata.AttributeTypeCode.Lookup:
                            case Microsoft.Xrm.Sdk.Metadata.AttributeTypeCode.Customer:
                            case Microsoft.Xrm.Sdk.Metadata.AttributeTypeCode.Owner:
                                var lookupReference = new EntityReference(
                                    ((Microsoft.Xrm.Sdk.Metadata.LookupAttributeMetadata)attributeMetadata).Targets[0],
                                    Guid.Parse(attribute.Value));
                                typedValue = lookupReference;
                                break;
                            case Microsoft.Xrm.Sdk.Metadata.AttributeTypeCode.Uniqueidentifier:
                                typedValue = Guid.Parse(attribute.Value);
                                break;
                            default:
                                typedValue = attribute.Value;
                                break;
                        }

                        entity.Attributes.Add(attribute.Key.ToLower(), typedValue);
                    }
                    catch (Exception ex)
                    {
                        // If metadata retrieval fails, add as string
                        entity.Attributes.Add(attribute.Key, attribute.Value);
                        Console.WriteLine($"Warning: Couldn't determine type for {attribute.Key}: {ex.Message}");
                    }
                }

                // Check if record with the ID exists before creating
                bool recordExists = false;
                if (recordId != Guid.Empty)
                {
                    try
                    {
                        // Try to retrieve the record
                        var retrieveRequest = new Microsoft.Xrm.Sdk.Messages.RetrieveRequest
                        {
                            Target = new EntityReference(tableName, recordId),
                            ColumnSet = new Microsoft.Xrm.Sdk.Query.ColumnSet(false) // Just need to check existence, not retrieving attributes
                        };

                        var retrieveResponse = (Microsoft.Xrm.Sdk.Messages.RetrieveResponse)serviceClient.Execute(retrieveRequest);

                        // If we get here, record exists
                        Console.WriteLine($"Record with ID {recordId} already exists in {tableName}.");
                        recordExists = true;
                    }
                    catch (System.ServiceModel.FaultException<Microsoft.Xrm.Sdk.OrganizationServiceFault> ex)
                    {
                        if (ex.Message.Contains("Does Not Exist"))
                        {
                            Console.WriteLine($"Record with ID {recordId} does not exist in {tableName}. Will create new record.");
                            recordExists = false;
                        }
                        else
                        {
                            throw; // Re-throw if it's a different error
                        }
                    }
                }

                // Create or update the record based on existence check
                if (recordExists)
                {
                    string overwrite = "";

                    var updateRecord = await server.ElicitAsync(
               new()
               {
                   Message = $"A {tableName} with ID {recordId} already exists. Do you want to overwrite it?",
                   RequestedSchema = new()
                   {
                       Properties = new Dictionary<string, ElicitRequestParams.PrimitiveSchemaDefinition>()
                       {
                           ["overwrite"] = new ElicitRequestParams.EnumSchema
                           {
                               Title = "Overwrite?",
                               Enum = ["1", "2"],
                               EnumNames = ["Yes", "No"],
                           }
                       },
                   },
               },
               CancellationToken.None);
                    if (updateRecord.Content != null && updateRecord.Content.TryGetValue("overwrite", out var overwriteValue))
                    {
                        overwrite = overwriteValue.ToString();

                        if (overwrite == "1")
                        {
                            // Update existing record
                            serviceClient.Update(entity);
                            overwritten.Add(recordId);
                        }
                        else
                        {
                            skipped.Add(recordId);
                            continue;
                        }
                    }
                }
                else
                {
                    // Create new record
                    serviceClient.Create(entity);
                }

                // Update progress if we have a progress token
                if (progressToken != null)
                {
                    await server.NotifyProgressAsync(progressToken.Value, new()
                    {
                        Progress = i + 1,
                        Message = $"Migrated record {i + 1}/{totalRecords}: {recordDetails}",
                        Total = totalRecords
                    });
                }
            }

            return $"Successfully migrated {totalRecords - skipped.Count} records to the {tableName} table. The following GUIDs were skipped: {string.Join(", ", skipped)}";
        }
        catch (Newtonsoft.Json.JsonException ex)
        {
            return $"Error parsing record list JSON: {ex.Message}";
        }
        catch (Exception ex)
        {
            return $"Error during migration: {ex.Message}";
        }
    }

}