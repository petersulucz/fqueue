$global:fqueueSocketEndpoint = $null;

function Connect
{
    param
    (
        [Parameter(Mandatory=$false)]$IP = '127.0.0.1',
        [Parameter(Mandatory=$true)]$Port
    )
    process
    {
        $endpoint = [System.Net.IPEndPoint]::new([System.Net.IPAddress]::Parse($IP), $Port);
        $global:fqueueSocketEndpoint = $endpoint
    }
}

function GetSocket
{
    $sock = [System.Net.Sockets.TcpClient]::new();
    return $sock;
}


function Queue-Push
{
    param
    (
        [Parameter(Mandatory=$true)]$Queue,
        [Parameter(Mandatory=$true)]$Data
    )
    process
    {
        $obj = "{ `"type`": `"push`", `"queue`": `"$Queue`", `"data`": `"$data`"}`n`n"
        $sock = GetSocket

        $sock.Connect($global:fqueueSocketEndpoint);
        $str = $sock.GetStream();

        $bytes = [System.Text.UnicodeEncoding]::Unicode.GetBytes($obj)
        $str.Write($bytes, 0, $bytes.Length);

        $str.Dispose();
        $sock.Dispose();
    }
}

function Queue-Pop
{
    param
    (
        [Parameter(Mandatory=$true)]$Queue
    )
    process
    {
        $obj = "{ `"type`": `"pop`", `"queue`": `"$Queue`"}`n`n"
        $sock = GetSocket

        $sock.Connect($global:fqueueSocketEndpoint);
        $str = $sock.GetStream();

        $bytes = [System.Text.UnicodeEncoding]::Unicode.GetBytes($obj)
        $str.Write($bytes, 0, $bytes.Length);

        $reader = [System.IO.StreamReader]::new($str, [System.Text.Encoding]::Unicode)

        $text = $reader.ReadToEnd();

        Write-Output $text

        $reader.Dispose();
        $str.Dispose();
        $sock.Dispose();
    }
}

