Imports System.Threading.Tasks
Imports MySql.Data.MySqlClient
Imports Newtonsoft.Json.Linq
Imports RestSharp

Public Class EtherController
    Inherits System.Web.Mvc.Controller

    Public Async Function Index() As Task(Of ActionResult)

        Dim logger = NLog.LogManager.GetCurrentClassLogger()
        Dim client = New RestClient("https://api.etherscan.io")
        Dim request = New RestRequest("/api", Method.Get)

        Dim transactions = New List(Of JObject)

        logger.Info("Start eth_getBlockByNumber Block 12100001 to 12100500 ")
        For i As Integer = 12100001 To 12100003
            request.AddOrUpdateParameter("module", "proxy")
            request.AddOrUpdateParameter("action", "eth_getBlockByNumber")
            request.AddOrUpdateParameter("tag", "0x" + Hex(i).ToString())
            request.AddOrUpdateParameter("boolean", "true")
            request.AddOrUpdateParameter("apikey", "RCGPGHYVW68ZITT9SX7UCCSJPGN6QBCH67")

            Dim response = Await client.ExecuteAsync(request)

            If response.StatusCode = Net.HttpStatusCode.OK Then

                logger.Info("Block #" + i.ToString() + " has value found.")
                Dim result = JObject.Parse(response.Content)
                logger.Info("Block #" + result("result")("hash").ToString() + " has value found.")
                client = New RestClient("https://api.etherscan.io")
                Dim eth_getBlockTransactionCountByNumberRequest = New RestRequest("/api", Method.Get)
                eth_getBlockTransactionCountByNumberRequest.AddOrUpdateParameter("module", "proxy")
                eth_getBlockTransactionCountByNumberRequest.AddOrUpdateParameter("action", "eth_getBlockTransactionCountByNumber")
                eth_getBlockTransactionCountByNumberRequest.AddOrUpdateParameter("tag", "0x" + Hex(i).ToString())
                eth_getBlockTransactionCountByNumberRequest.AddOrUpdateParameter("boolean", "true")
                eth_getBlockTransactionCountByNumberRequest.AddOrUpdateParameter("apikey", "RCGPGHYVW68ZITT9SX7UCCSJPGN6QBCH67")

                Dim eth_getBlockTransactionCountByNumberRes = Await client.ExecuteAsync(eth_getBlockTransactionCountByNumberRequest)
                Dim responseContent2 As String = eth_getBlockTransactionCountByNumberRes.Content
                Dim transactionCount As Integer = Convert.ToInt32(JObject.Parse(responseContent2)("result").ToString(), 16)



                'Insert the block details into the MySQL database
                Dim connectionString As String = "Server=localhost;Database=EtherDb;Uid=root;Pwd=admin;"
                Dim sql As String = "INSERT INTO Blocks(blockNumber, hash, parentHash, miner, blockReward, gasLimit, gasUsed) VALUES(@blockNumber, @hash, @parentHash, @miner, @blockReward, @gasLimit, @gasUsed)"
                Using connection As New MySqlConnection(connectionString)
                    connection.Open()

                    Dim command As New MySqlCommand(sql, connection)
                    command.Parameters.AddWithValue("@blockNumber", i)
                    command.Parameters.AddWithValue("@hash", result("result")("hash").ToString())
                    command.Parameters.AddWithValue("@parentHash", result("result")("parentHash").ToString())
                    command.Parameters.AddWithValue("@miner", result("result")("miner").ToString())
                    command.Parameters.AddWithValue("@blockReward", 0)
                    command.Parameters.AddWithValue("@gasLimit", Convert.ToDecimal(Convert.ToInt64(result("result")("gasLimit").ToString(), 16)))
                    command.Parameters.AddWithValue("@gasUsed", Convert.ToDecimal(Convert.ToInt64(result("result")("gasUsed").ToString(), 16)))
                    Dim rowAffected As Integer = command.ExecuteNonQuery()
                    Dim blockID As Integer = Convert.ToInt32(command.LastInsertedId)

                    If rowAffected > 0 Then
                        logger.Info("Block inserted successfully.")
                        connection.Close()
                    Else
                        logger.Info("Block did not inserted successfully.")
                    End If

                    logger.Info("[TransactionCount][eth_getBlockTransactionCountByNumber] has count " + transactionCount.ToString())
                    If (transactionCount > 0) Then
                        For k As Integer = 0 To transactionCount - 1
                            client = New RestClient("https://api.etherscan.io")
                            Dim eth_getTransactionByBlockNumberAndIndexRequest = New RestRequest("/api", Method.Get)
                            eth_getTransactionByBlockNumberAndIndexRequest.AddOrUpdateParameter("module", "proxy")
                            eth_getTransactionByBlockNumberAndIndexRequest.AddOrUpdateParameter("action", "eth_getTransactionByBlockNumberAndIndex")
                            eth_getTransactionByBlockNumberAndIndexRequest.AddOrUpdateParameter("tag", "0x" + Hex(i).ToString())
                            eth_getTransactionByBlockNumberAndIndexRequest.AddOrUpdateParameter("index", "0x" + Hex(k).ToString())
                            eth_getTransactionByBlockNumberAndIndexRequest.AddOrUpdateParameter("apikey", "RCGPGHYVW68ZITT9SX7UCCSJPGN6QBCH67")

                            Dim response1 = Await client.ExecuteAsync(eth_getTransactionByBlockNumberAndIndexRequest)
                            Dim blockInfoResponse = JObject.Parse(response1.Content)
                            Dim transactionsSql As String = "INSERT INTO transactions (blockID, hash, fromAddress, toAddress, value, gas, gasPrice, transactionIndex) VALUES (@blockID, @hash, @fromAddress, @toAddress, @value, @gas, @gasPrice, @transactionIndex)"
                            Using connection1 As New MySqlConnection(connectionString)
                                connection1.Open()
                                Dim command1 As New MySqlCommand(transactionsSql, connection1)
                                command1.Parameters.AddWithValue("@blockID", blockID)
                                command1.Parameters.AddWithValue("@hash", blockInfoResponse("result")("hash").ToString())
                                command1.Parameters.AddWithValue("@fromAddress", blockInfoResponse("result")("from").ToString())
                                command1.Parameters.AddWithValue("@toAddress", blockInfoResponse("result")("to").ToString())
                                command1.Parameters.AddWithValue("@value", Convert.ToDecimal(Convert.ToInt64(blockInfoResponse("result")("value").ToString(), 16)))
                                command1.Parameters.AddWithValue("@gas", Convert.ToDecimal(Convert.ToInt64(blockInfoResponse("result")("gas").ToString(), 16)))
                                command1.Parameters.AddWithValue("@gasPrice", Convert.ToDecimal(Convert.ToInt64(blockInfoResponse("result")("gasPrice").ToString(), 16)))
                                command1.Parameters.AddWithValue("@transactionIndex", Convert.ToInt32(blockInfoResponse("result")("transactionIndex").ToString(), 16))
                                Dim rowsAffected1 As Integer = command1.ExecuteNonQuery()

                                If rowsAffected1 > 0 Then
                                    logger.Info("[eth_getTransactionByBlockNumberAndIndex][Transaction Index:" + Convert.ToInt32(blockInfoResponse("result")("transactionIndex").ToString(), 16).ToString() + "]Transaction inserted successfully.")
                                    connection1.Close()
                                Else
                                    logger.Info("Transaction did not inserted successfully.")
                                End If
                            End Using
                        Next


                    End If



                End Using
            Else
                Console.WriteLine("Block not found")
            End If

        Next

        ViewBag.Transactions = transactions
        logger.Info("API RUN FINISH SUCCESSFULLY.")
        Return View()
    End Function

End Class
