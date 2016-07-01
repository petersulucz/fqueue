# The endpoint
$global:fqueueSocketEndpoint = $null;

function Queue-Connect
{
    param
    (
        [Parameter(Mandatory=$false)][String]$IP = '127.0.0.1',
        [Parameter(Mandatory=$true)][Int]$Port
    )
    process
    {
        $endpoint = [System.Net.IPEndPoint]::new([System.Net.IPAddress]::Parse($IP), $Port);
        $global:fqueueSocketEndpoint = $endpoint
    }
}

# Push an item onto the queue
function Queue-Push
{
    [CmdletBinding()]
    param
    (
        [Parameter(Mandatory=$true)][String]$Queue,
        [Parameter(Mandatory=$true)][String]$Data,
        [Parameter(Mandatory=$false)][System.Net.IPEndPoint]$Endpoint = $global:fqueueSocketEndpoint
    )
    process
    {
        # Make sure the endpoint is defined
        if(-not $Endpoint)
        {
            throw 'The endpoint has not been defined. Please call Queue-Connect'
        }


        $obj = "{ `"type`": `"push`", `"queue`": `"$Queue`", `"data`": `"$data`"}`n`n"
        $sock = $sock = [System.Net.Sockets.TcpClient]::new();

        $sock.Connect($global:fqueueSocketEndpoint);
        $str = $sock.GetStream();

        $bytes = [System.Text.UnicodeEncoding]::Unicode.GetBytes($obj)
        $str.Write($bytes, 0, $bytes.Length);

        # Read all our output
        $reader = [System.IO.StreamReader]::new($str, [System.Text.Encoding]::Unicode)
        $text = $reader.ReadToEnd();

        $reader.Dispose();

        $str.Dispose();
        $sock.Dispose();

        return $text
    }
}

# Pop an item from the queue
function Queue-Pop
{
    [CmdletBinding()]
    param
    (
        [Parameter(Mandatory=$true)][String]$Queue,
        [Parameter(Mandatory=$false)][System.Net.IPEndPoint]$Endpoint = $global:fqueueSocketEndpoint
    )
    process
    {
        # Make sure the endpoint is defined
        if(-not $Endpoint)
        {
            throw 'The endpoint has not been defined. Please call Queue-Connect'
        }

        # Stuff to send
        $obj = "{ `"type`": `"pop`", `"queue`": `"$Queue`"}`n`n"
        $sock = $sock = [System.Net.Sockets.TcpClient]::new();

        $sock.Connect($global:fqueueSocketEndpoint);
        $str = $sock.GetStream();

        # Get the text to send
        $bytes = [System.Text.UnicodeEncoding]::Unicode.GetBytes($obj)
        $str.Write($bytes, 0, $bytes.Length);

        # Read all our output
        $reader = [System.IO.StreamReader]::new($str, [System.Text.Encoding]::Unicode)
        $text = $reader.ReadToEnd();

        $reader.Dispose();
        $str.Dispose();
        $sock.Dispose();

        return $text
    }
}

