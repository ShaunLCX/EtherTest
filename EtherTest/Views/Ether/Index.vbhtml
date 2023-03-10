@ModelType IEnumerable(Of Newtonsoft.Json.Linq.JObject)
@Code
    ViewData("Title") = "Index"
End Code

<h1>Ethereum Transactions from Blocks #12100001 to #12100500</h1>

<table>
    <thead>
        <tr>
            <th>Block Number</th>
            <th>Transaction Hash</th>
            <th>From</th>
            <th>To</th>
            <th>Value</th>
        </tr>
    </thead>
    <tbody>
        @For Each tx In Model
            @<tr>
                <td>@Convert.ToInt32(tx("blockNumber").ToString(), 16)</td>
                <td>@tx("hash")</td>
                <td>@tx("from")</td>
                <td>@tx("to")</td>
            </tr>
        Next
    </tbody>
</table>
