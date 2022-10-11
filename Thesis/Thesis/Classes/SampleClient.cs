using Opc.Ua;
using Opc.Ua.Client;
using Opc.Ua.Configuration;
using System;
using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;
using Xamarin.Forms;

namespace Thesis
{
    public class SampleClient
    {
        public enum ConnectionStatus
        {
            None,
            NotConnected,
            Connected,
            Error
        }

        public ConnectionStatus connectionStatus;
        public bool haveAppCertificate;
        public Session session = null;
        private SessionReconnectHandler reconnectHandler;
        private const int ReconnectPeriod = 10;

        private LabelViewModel info;
        private ApplicationConfiguration config;

        public SampleClient(LabelViewModel text)
        {
            connectionStatus = ConnectionStatus.None;
            session = null;
            info = text;
            haveAppCertificate = false;
            config = null;
        }
        //public Session Session
        //{
        //    get { return session; }
        //}
        /// <summary>
        /// Provides the event handling for server certificates.
        /// </summary>
        public CertificateValidationEventHandler CertificateValidationNotification = null;


        /// <summary>
        /// Provides the event for a monitored event item.
        /// </summary>
        public NotificationEventHandler ItemEventNotification = null;

        /// <summary>
        /// Provides the event for KeepAliveNotifications.
        /// </summary>
        public KeepAliveEventHandler KeepAliveNotification = null;
        public async void CreateCertificate()
        {
            ApplicationInstance application = new ApplicationInstance
            {
                ApplicationType = ApplicationType.Client,
                ConfigSectionName = "Opc.Ua.SampleClient"
            };

            if (Device.RuntimePlatform == "Android")
            {
                string currentFolder = DependencyService.Get<IPathService>().PublicExternalFolder.ToString();
                string filename = application.ConfigSectionName + ".Config.xml";
                string content = DependencyService.Get<IAssetService>().LoadFile(filename);

                File.WriteAllText(currentFolder + filename, content);
                // load the application configuration.
                config = await application.LoadApplicationConfiguration(currentFolder + filename, false);
            }
            else
            {
                // load the application configuration.
                config = await application.LoadApplicationConfiguration(false);
            }

            // check the application certificate.
            haveAppCertificate = await application.CheckApplicationInstanceCertificate(false, 0);

            switch (Device.RuntimePlatform)
            {
                case "Android":
                    config.ApplicationName = "OPC UA Xamarin Sample Client Android";
                    break;

                case "UWP":
                    config.ApplicationName = "OPC UA Xamarin Sample Client UWP";
                    break;

                case "iOS":
                    config.ApplicationName = "OPC UA Xamarin Sample Client IOS";
                    break;
            }

            if (haveAppCertificate)
            {
                config.ApplicationUri = Utils.GetApplicationUriFromCertificate(config.SecurityConfiguration.ApplicationCertificate.Certificate);

                config.CertificateValidator.CertificateValidation += new CertificateValidationEventHandler(CertificateValidator_CertificateValidation);
            }
        }

        public async Task<ConnectionStatus> OpcClient(string endpointURL, bool userAuth, string userName, string password)
        {
            try
            {
                Uri endpointURI = new Uri(endpointURL);
                var selectedEndpoint = CoreClientUtils.SelectEndpoint(endpointURL, haveAppCertificate, 15000);

                info.LabelText = "Selected endpoint uses: " + selectedEndpoint.SecurityPolicyUri.Substring(selectedEndpoint.SecurityPolicyUri.LastIndexOf('#') + 1);

                var endpointConfiguration = EndpointConfiguration.Create(config);
                var endpoint = new ConfiguredEndpoint(selectedEndpoint.Server, endpointConfiguration);
                endpoint.Update(selectedEndpoint);

                var platform = Device.RuntimePlatform;
                var sessionName = "";

                switch (Device.RuntimePlatform)
                {
                    case "Android":
                        sessionName = "OPC UA Xamarin Client Android";
                        break;

                    case "UWP":
                        sessionName = "OPC UA Xamarin Client UWP";
                        break;

                    case "iOS":
                        sessionName = "OPC UA Xamarin Client IOS";
                        break;
                }
                UserIdentity UserIdentity;
                if (userAuth)
                {
                    UserIdentity = new UserIdentity(userName, password);
                }
                else
                {
                    UserIdentity = new UserIdentity();
                }
                session = await Session.Create(config, endpoint, true, sessionName, 60000, UserIdentity, null);

                if (session != null)
                {
                    connectionStatus = ConnectionStatus.Connected;
                }
                else
                {
                    connectionStatus = ConnectionStatus.NotConnected;
                }
                // register keep alive handler
                session.KeepAlive += Client_KeepAlive;
            }
            catch
            {
                connectionStatus = ConnectionStatus.Error;
            }
            return connectionStatus;
        }

        public void Disconnect(Session session)
        {
            if (session != null)
            {
                if (info != null)
                {
                    info.LabelText = "";
                }

                session.Close();
            }
        }

        private void Client_KeepAlive(Session sender, KeepAliveEventArgs e)
        {
            if (e.Status != null && ServiceResult.IsNotGood(e.Status))
            {
                info.LabelText = e.Status.ToString() + sender.OutstandingRequestCount.ToString() + "/" + sender.DefunctRequestCount.ToString();

                if (reconnectHandler == null)
                {
                    info.LabelText = "--- RECONNECTING ---";
                    reconnectHandler = new SessionReconnectHandler();
                    reconnectHandler.BeginReconnect(sender, ReconnectPeriod * 1000, Client_ReconnectComplete);
                }
            }
        }

        private void Client_ReconnectComplete(object sender, EventArgs e)
        {
            // ignore callbacks from discarded objects.
            if (!Object.ReferenceEquals(sender, reconnectHandler))
            {
                return;
            }

            session = reconnectHandler.Session;
            reconnectHandler.Dispose();
            reconnectHandler = null;

            info.LabelText = "--- RECONNECTING ---";
        }

        public Tree GetRootNode(LabelViewModel textInfo)
        {
            ReferenceDescriptionCollection references;
            Byte[] continuationPoint;
            Tree browserTree = new Tree();

            try
            {
                session.Browse(
                    null,
                    null,
                    ObjectIds.ObjectsFolder,
                    0u,
                    BrowseDirection.Forward,
                    ReferenceTypeIds.HierarchicalReferences,
                    true,
                    0,
                    out continuationPoint,
                    out references);

                browserTree.currentView.Add(new ListNode { id = ObjectIds.ObjectsFolder.ToString(), NodeName = "Objects", children = (references?.Count != 0) });
                browserTree.currentView.Add(new ListNode { id = ObjectIds.TypesFolder.ToString(), NodeName = "Types", children = (references?.Count != 0) });
                browserTree.currentView.Add(new ListNode { id = ObjectIds.ViewsFolder.ToString(), NodeName = "Views", children = (references?.Count != 0) });
                return browserTree;
            }
            catch
            {
                Disconnect(session);
                return null;
            }
        }

        public Tree GetChildren(string node)
        {
            ReferenceDescriptionCollection references;
            ReferenceDescriptionCollection nextreferenceDescriptionCollection;
            Byte[] continuationPoint;
            Tree browserTree = new Tree();
            Byte[] revisedContinuationPoint;

            try
            {
                session.Browse(
                    null,
                    null,
                    node,
                    0u,
                    BrowseDirection.Forward,
                    ReferenceTypeIds.HierarchicalReferences,
                    true,
                    0,
                    out continuationPoint,
                    out references);

                while (continuationPoint != null)
                {
                    session.BrowseNext(null, false, continuationPoint, out revisedContinuationPoint, out nextreferenceDescriptionCollection);
                    references.AddRange(nextreferenceDescriptionCollection);
                    continuationPoint = revisedContinuationPoint;
                }

                if (references != null)
                {
                    foreach (var nodeReference in references)
                    {
                        ReferenceDescriptionCollection childReferences = null;
                        Byte[] childContinuationPoint;

                        session.Browse(
                            null,
                            null,
                            ExpandedNodeId.ToNodeId(nodeReference.NodeId, session.NamespaceUris),
                            0u,
                            BrowseDirection.Forward,
                            ReferenceTypeIds.HierarchicalReferences,
                            true,
                            0,
                            out childContinuationPoint,
                            out childReferences);

                        INode currentNode = null;
                        try
                        {
                            currentNode = session.ReadNode(ExpandedNodeId.ToNodeId(nodeReference.NodeId, session.NamespaceUris));
                        }
                        catch (Exception)
                        {
                            // skip this node
                            continue;
                        }

                        byte currentNodeAccessLevel = 0;
                        byte currentNodeEventNotifier = 0;
                        bool currentNodeExecutable = false;

                        VariableNode variableNode = currentNode as VariableNode;
                        if (variableNode != null)
                        {
                            currentNodeAccessLevel = variableNode.UserAccessLevel;
                            currentNodeAccessLevel = (byte)((uint)currentNodeAccessLevel & ~0x2);
                        }

                        ObjectNode objectNode = currentNode as ObjectNode;
                        if (objectNode != null)
                        {
                            currentNodeEventNotifier = objectNode.EventNotifier;
                        }

                        ViewNode viewNode = currentNode as ViewNode;
                        if (viewNode != null)
                        {
                            currentNodeEventNotifier = viewNode.EventNotifier;
                        }

                        MethodNode methodNode = currentNode as MethodNode;
                        if (methodNode != null)
                        {
                            currentNodeExecutable = methodNode.UserExecutable;
                        }

                        browserTree.currentView.Add(new ListNode()
                        {
                            id = nodeReference.NodeId.ToString(),
                            NodeName = nodeReference.DisplayName.Text.ToString(),
                            nodeClass = nodeReference.NodeClass.ToString(),
                            accessLevel = currentNodeAccessLevel.ToString(),
                            eventNotifier = currentNodeEventNotifier.ToString(),
                            executable = currentNodeExecutable.ToString(),
                            children = (references?.Count != 0),
                            ImageUrl = (nodeReference.NodeClass.ToString() == "Variable") ? "tag.png" : "folder.png"
                        });
                        if (browserTree.currentView[0].ImageUrl == null)
                        {
                            browserTree.currentView[0].ImageUrl = "";
                        }
                    }
                    if (browserTree.currentView.Count == 0)
                    {
                        INode currentNode = session.ReadNode(new NodeId(node));

                        byte currentNodeAccessLevel = 0;
                        byte currentNodeEventNotifier = 0;
                        bool currentNodeExecutable = false;

                        VariableNode variableNode = currentNode as VariableNode;

                        if (variableNode != null)
                        {
                            currentNodeAccessLevel = variableNode.UserAccessLevel;
                            currentNodeAccessLevel = (byte)((uint)currentNodeAccessLevel & ~0x2);
                        }

                        ObjectNode objectNode = currentNode as ObjectNode;

                        if (objectNode != null)
                        {
                            currentNodeEventNotifier = objectNode.EventNotifier;
                        }

                        ViewNode viewNode = currentNode as ViewNode;

                        if (viewNode != null)
                        {
                            currentNodeEventNotifier = viewNode.EventNotifier;
                        }

                        MethodNode methodNode = currentNode as MethodNode;

                        if (methodNode != null)
                        {
                            currentNodeExecutable = methodNode.UserExecutable;
                        }

                        browserTree.currentView.Add(new ListNode()
                        {
                            id = node,
                            NodeName = currentNode.DisplayName.Text.ToString(),
                            nodeClass = currentNode.NodeClass.ToString(),
                            accessLevel = currentNodeAccessLevel.ToString(),
                            eventNotifier = currentNodeEventNotifier.ToString(),
                            executable = currentNodeExecutable.ToString(),
                            children = false,
                            ImageUrl = null
                        });
                    }
                }
                return browserTree;
            }
            catch
            {
                Disconnect(session);
                return null;
            }
        }

        public Node ReadNode(String nodeIdString)
        {
            //Create a nodeId using the identifier string
            NodeId nodeId = new NodeId(nodeIdString);
            //Create a node
            Node node = new Node();
            try
            {
                //Read the dataValue
                node = session.ReadNode(nodeId);
                return node;
            }
            catch (Exception e)
            {
                //handle Exception here
                throw e;
            }
        }

        public string VariableRead(string node)
        {
            try
            {
                DataValueCollection values = null;
                DiagnosticInfoCollection diagnosticInfos = null;
                ReadValueIdCollection nodesToRead = new ReadValueIdCollection();
                ReadValueId valueId = new ReadValueId();
                valueId.NodeId = new NodeId(node);
                valueId.AttributeId = Attributes.Value;
                valueId.IndexRange = null;
                valueId.DataEncoding = null;
                nodesToRead.Add(valueId);
                ResponseHeader responseHeader = session.Read(null, 0, TimestampsToReturn.Both, nodesToRead, out values, out diagnosticInfos);
                string value = "";
                if (values[0].Value != null)
                {
                    var rawValue = values[0].WrappedValue.ToString();
                    value = rawValue.Replace("|", "\r\n").Replace("{", "").Replace("}", "");
                }
                return value;
            }
            catch
            {
                return null;
            }
        }

        public void Read_Datatype(SampleClient opcClient, string id, out List<string> displayNames, out VariableNode variableNode)
        {
            Node node = opcClient.ReadNode(id);
            variableNode = new VariableNode();
            variableNode = (VariableNode)node.DataLock;
            List<NodeId> nodeIds = new List<NodeId>();
            displayNames = new List<string>();
            List<ServiceResult> errors = new List<ServiceResult>();
            NodeId nodeId = new NodeId(variableNode.DataType);
            nodeIds.Add(nodeId);
            session.ReadDisplayName(nodeIds, out displayNames, out errors);
        }

        public void VariableWrite(List<String> values, List<String> nodeIdStrings)
        {
            //Create a collection of values to write
            WriteValueCollection valuesToWrite = new WriteValueCollection();
            //Create a collection for StatusCodes
            StatusCodeCollection result = new StatusCodeCollection();
            //Create a collection for DiagnosticInfos
            DiagnosticInfoCollection diagnostics = new DiagnosticInfoCollection();

            foreach (String str in nodeIdStrings)
            {
                //Create a nodeId
                NodeId nodeId = new NodeId(str);
                //Create a dataValue
                DataValue dataValue = new DataValue();
                //Read the dataValue
                try
                {
                    dataValue = session.ReadValue(nodeId);
                }
                catch (Exception e)
                {
                    //handle Exception here
                    throw e;
                }

                string test = dataValue.Value.GetType().Name;
                //Get the data type of the read dataValue
                //Handle Arrays here: TBD
                Variant variant = 0;
                try
                {
                    variant = new Variant(Convert.ChangeType(values[nodeIdStrings.IndexOf(str)], dataValue.Value.GetType()));
                }
                catch //no base data type
                {
                    //Handle different arrays types here: TBD
                    if (dataValue.Value.GetType().Name == "string[]")
                    {
                        string[] arrString = values[nodeIdStrings.IndexOf(str)].Split(';');
                        variant = new Variant(arrString);
                    }
                    else if (dataValue.Value.GetType().Name == "Byte[]")
                    {
                        string[] arrString = values[nodeIdStrings.IndexOf(str)].Split(';');
                        Byte[] arrInt = new Byte[arrString.Length];

                        for (int i = 0; i < arrString.Length; i++)
                        {
                            arrInt[i] = Convert.ToByte(arrString[i]);
                        }
                        variant = new Variant(arrInt);
                    }
                    else if (dataValue.Value.GetType().Name == "Int16[]")
                    {
                        string[] arrString = values[nodeIdStrings.IndexOf(str)].Split(';');
                        Int16[] arrInt = new Int16[arrString.Length];

                        for (int i = 0; i < arrString.Length; i++)
                        {
                            arrInt[i] = Convert.ToInt16(arrString[i]);
                        }
                        variant = new Variant(arrInt);
                    }
                }

                //Overwrite the dataValue with a new constructor using read dataType
                dataValue = new DataValue(variant);

                //Create a WriteValue using the NodeId, dataValue and attributeType
                WriteValue valueToWrite = new WriteValue();
                valueToWrite.Value = dataValue;
                valueToWrite.NodeId = nodeId;
                valueToWrite.AttributeId = Attributes.Value;

                //Add the dataValues to the collection
                valuesToWrite.Add(valueToWrite);
            }

            try
            {
                //Write the collection to the server
                session.Write(null, valuesToWrite, out result, out diagnostics);
                foreach (StatusCode code in result)
                {
                    if (code != 0)
                    {
                        Exception ex = new Exception(code.ToString());
                        throw ex;
                    }
                }
            }
            catch (Exception e)
            {
                //handle Exception here
                throw e;
            }
        }

        private void CertificateValidator_CertificateValidation(CertificateValidator validator, CertificateValidationEventArgs e)
        {
            if (e.Error.StatusCode == StatusCodes.BadCertificateUntrusted)
            {
                e.Accept = config.SecurityConfiguration.AutoAcceptUntrustedCertificates;
                if (config.SecurityConfiguration.AutoAcceptUntrustedCertificates)
                {
                    info.LabelText = "Accepted Certificate: " + e.Certificate.Subject.ToString();
                }
                else
                {
                    info.LabelText = "Rejected Certificate: " + e.Certificate.Subject.ToString();
                }
            }
        }

        public MonitoredItem RemoveMonitoredItem(Subscription subscription, MonitoredItem monitoredItem)
        {
            try
            {
                //Add the item to the subscription
                subscription.RemoveItem(monitoredItem);
                //Apply changes to the subscription
                subscription.ApplyChanges();
                return null;
            }
            catch (Exception e)
            {
                //handle Exception here
                throw e;
            }
        }

        public void RemoveSubscription(Subscription subscription)
        {
            try
            {
                //Delete the subscription and all items submitted
                subscription.Delete(true);
            }
            catch (Exception e)
            {
                //handle Exception here
                throw e;
            }
        }

        public Subscription Subscribe(int publishingInterval)
        {
            //Create a Subscription object
            Subscription subscription = new Subscription(session.DefaultSubscription);
            //Enable publishing
            subscription.PublishingEnabled = true;
            //Set the publishing interval
            subscription.PublishingInterval = publishingInterval;
            //Add the subscription to the session
            session.AddSubscription(subscription);
            try
            {
                //Create/Activate the subscription
                subscription.Create();
                return subscription;
            }
            catch (Exception e)
            {
                //handle Exception here
                throw e;
            }
        }

        public MonitoredItem AddMonitoredItem(Subscription subscription, string nodeIdString, string itemName, int samplingInterval)
        {
            //Create a monitored item
            MonitoredItem monitoredItem = new MonitoredItem();
            //Set the name of the item for assigning items and values later on; make sure item names differ
            monitoredItem.DisplayName = itemName;
            //Set the NodeId of the item
            monitoredItem.StartNodeId = nodeIdString;
            //Set the attribute Id (value here)
            monitoredItem.AttributeId = Attributes.Value;
            //Set reporting mode
            monitoredItem.MonitoringMode = MonitoringMode.Reporting;
            //Set the sampling interval (1 = fastest possible)
            monitoredItem.SamplingInterval = samplingInterval;
            //Set the queue size
            monitoredItem.QueueSize = 1;
            //Discard the oldest item after new one has been received
            monitoredItem.DiscardOldest = true;
            //Define event handler for this item and then add to monitoredItem
            monitoredItem.Notification += new MonitoredItemNotificationEventHandler(Notification_MonitoredItem);
            try
            {
                //Add the item to the subscription
                subscription.AddItem(monitoredItem);
                //Apply changes to the subscription
                subscription.ApplyChanges();
                return monitoredItem;
            }
            catch (Exception e)
            {
                //handle Exception here
                throw e;
            }
        }

        public MonitoredItemNotificationEventHandler ItemChangedNotification = null;

        private void Notification_MonitoredItem(MonitoredItem monitoredItem, MonitoredItemNotificationEventArgs e)
        {
            ItemChangedNotification(monitoredItem, e);
        }

        public IList<object> CallMethod(String methodIdString, String objectIdString, List<string[]> inputData)
        {
            //For calling a method we need it's node id and it's parent object's node id
            NodeId methodNodeId = new NodeId(methodIdString);
            NodeId objectNodeId = new NodeId(objectIdString);

            //Declare an array of objects for the method's input arguments
            Object[] inputArguments = new object[inputData.Count];

            //Parse data types first
            //TBD: arrays for all types

            Boolean parseCheck = false;
            for (int i = 0; i < inputData.Count; i++)
            {
                if (inputData[i][1] == "SByte")
                {
                    SByte value = 0;
                    parseCheck = SByte.TryParse(inputData[i][0], out value);
                    inputArguments[i] = value;
                }
                else if (inputData[i][1] == "Byte")
                {
                    Byte value = 0;
                    parseCheck = Byte.TryParse(inputData[i][0], out value);
                    inputArguments[i] = value;
                }
                else if (inputData[i][1] == "Int16")
                {
                    Int16 value = 0;
                    parseCheck = Int16.TryParse(inputData[i][0], out value);
                    inputArguments[i] = value;
                }
                else if (inputData[i][1].Contains("Int16["))
                {
                    int pFrom = inputData[i][1].IndexOf("[") + 1;
                    int pTo = inputData[i][1].LastIndexOf("]");
                    string tempString = inputData[i][1].Substring(pFrom, pTo - pFrom);
                    Int16[] value = new Int16[Int16.Parse(tempString)];

                    string[] tempArr = inputData[i][0].Split(';');

                    for (int j = 0; j < tempArr.Length; j++)
                    {
                        parseCheck = Int16.TryParse(tempArr[j], out value[j]);
                    }

                    inputArguments[i] = value;
                }
                else if (inputData[i][1] == "Int32")
                {
                    Int32 value = 0;
                    parseCheck = Int32.TryParse(inputData[i][0], out value);
                    inputArguments[i] = value;
                }
                else if (inputData[i][1] == "Int64")
                {
                    Int64 value = 0;
                    parseCheck = Int64.TryParse(inputData[i][0], out value);
                    inputArguments[i] = value;
                }
                else if (inputData[i][1] == "Boolean")
                {
                    Boolean value = false;
                    parseCheck = Boolean.TryParse(inputData[i][0], out value);
                    inputArguments[i] = value;
                }
                else if (inputData[i][1] == "String")
                {
                    parseCheck = true;
                    inputArguments[i] = inputData[i][0];
                }
                else if (inputData[i][1] == "Float")
                {
                    float value = 0;
                    parseCheck = float.TryParse(inputData[i][0], out value);
                    inputArguments[i] = value;
                }
                else if (inputData[i][1] == "DateTime")
                {
                    DateTime value = new DateTime();
                    parseCheck = DateTime.TryParse(inputData[i][0], out value);
                    inputArguments[i] = value;
                }
                else if (inputData[i][1] == "Double")
                {
                    Double value = 0;
                    parseCheck = Double.TryParse(inputData[i][0], out value);
                    inputArguments[i] = value;
                }
                else if (inputData[i][1] == "UInt16")
                {
                    UInt16 value = 0;
                    parseCheck = UInt16.TryParse(inputData[i][0], out value);
                    inputArguments[i] = value;
                }
                else if (inputData[i][1] == "UInt32")
                {
                    UInt32 value = 0;
                    parseCheck = UInt32.TryParse(inputData[i][0], out value);
                    inputArguments[i] = value;
                }
                else if (inputData[i][1] == "UInt64")
                {
                    UInt64 value = 0;
                    parseCheck = UInt64.TryParse(inputData[i][0], out value);
                    inputArguments[i] = value;
                }
                else if (inputData[i][1] == "Double")
                {
                    Double value = 0;
                    parseCheck = Double.TryParse(inputData[i][0], out value);
                    inputArguments[i] = value;
                }
                else if (inputData[i][1] == "ByteString")
                {
                    int NumberChars = inputData[i][0].Length;
                    if (NumberChars % 2 == 1)
                    {
                        Exception e = new Exception("Check length of ByteString");
                        throw e;
                    }
                    Byte[] value = new Byte[NumberChars / 2];
                    for (int j = 0; j < NumberChars; j += 2)
                    {
                        value[j / 2] = Convert.ToByte(inputData[i][0].Substring(j, 2), 16);
                    }
                    parseCheck = true;
                    inputArguments[i] = value;
                }
                else
                {
                    Exception e = new Exception("Complex data type cannot be parsed.");
                    throw e;
                }

                if (!parseCheck)
                {
                    Exception e = new Exception("Please validate input value");
                    throw e;
                }
            }

            //Declare a list of objects for the method's output arguments
            IList<object> outputArguments = new List<object>();

            //Call the method
            outputArguments = session.Call(objectNodeId, methodNodeId, inputArguments);

            return outputArguments;
        }
        public List<string> ReadValues(List<String> nodeIdStrings)
        {
            List<NodeId> nodeIds = new List<NodeId>();
            List<Type> types = new List<Type>();
            List<object> values = new List<object>();
            List<ServiceResult> serviceResults = new List<ServiceResult>();
            foreach (string str in nodeIdStrings)
            {
                //Create a nodeId using the identifier string and add to list
                nodeIds.Add(new NodeId(str));
                //No need for types
                types.Add(null);
            }
            try
            {
                //Read the dataValues
                session.ReadValues(nodeIds, types, out values, out serviceResults);
                //check ServiceResults to 
                foreach (ServiceResult svResult in serviceResults)
                {
                    if (svResult.ToString() != "Good")
                    {
                        Exception e = new Exception(svResult.ToString());
                        throw e;
                    }
                }
                List<string> resultStrings = new List<string>();
                foreach (object result in values)
                {
                    if (result != null)
                    {
                        if (result.ToString() == "System.Byte[]")
                        {
                            string str = "";
                            str = BitConverter.ToString((byte[])result).Replace("-", ";");
                            resultStrings.Add(str);
                        }
                        else if (result.ToString() == "System.String[]")
                        {
                            string str = "";
                            str = String.Join(";", (string[])result);
                            resultStrings.Add(str);
                        }
                        else if (result.ToString() == "System.Boolean[]")
                        {
                            string str = "";
                            foreach (Boolean intVar in (Boolean[])result)
                            {
                                str = str + ";" + intVar.ToString();
                            }
                            str = str.Remove(0, 1);
                            resultStrings.Add(str);
                        }
                        else if (result.ToString() == "System.Int16[]")
                        {
                            string str = "";
                            foreach (Int16 intVar in (Int16[])result)
                            {
                                str = str + ";" + intVar.ToString();
                            }
                            str = str.Remove(0, 1);
                            resultStrings.Add(str);
                        }
                        else if (result.ToString() == "System.UInt16[]")
                        {
                            string str = "";
                            foreach (UInt16 intVar in (UInt16[])result)
                            {
                                str = str + ";" + intVar.ToString();
                            }
                            str = str.Remove(0, 1);
                            resultStrings.Add(str);
                        }
                        else if (result.ToString() == "System.Int64[]")
                        {
                            string str = "";
                            foreach (Int64 intVar in (Int64[])result)
                            {
                                str = str + ";" + intVar.ToString();
                            }
                            str = str.Remove(0, 1);
                            resultStrings.Add(str);
                        }
                        else if (result.ToString() == "System.Single[]")
                        {
                            string str = "";
                            foreach (float intVar in (float[])result)
                            {
                                str = str + ";" + intVar.ToString();
                            }
                            str = str.Remove(0, 1);
                            resultStrings.Add(str);
                        }
                        else if (result.ToString() == "System.Double[]")
                        {
                            string str = "";
                            foreach (double intVar in (double[])result)
                            {
                                str = str + ";" + intVar.ToString();
                            }
                            str = str.Remove(0, 1);
                            resultStrings.Add(str);
                        }
                        else
                        {
                            resultStrings.Add(result.ToString());
                        }
                    }
                    else
                    {
                        resultStrings.Add("(null)");
                    }
                }
                return resultStrings;
            }
            catch (Exception e)
            {
                //handle Exception here
                throw e;
            }
        }
    }
}