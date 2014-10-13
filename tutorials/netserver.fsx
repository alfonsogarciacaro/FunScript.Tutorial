// From https://github.com/ZachBray/FunScript/blob/master/src/samples/Shared/Launcher.fs

// Change this value if you want to change the port
let port = 8080

type System.Net.HttpListener with
    member x.AsyncGetContext() = 
        Async.FromBeginEnd(x.BeginGetContext, x.EndGetContext)

type System.Net.HttpListenerRequest with
    member request.InputString =
        use sr = new System.IO.StreamReader(request.InputStream)
        sr.ReadToEnd()

type System.Net.HttpListenerResponse with
    member response.Reply(s:string) = 
        let buffer = System.Text.Encoding.UTF8.GetBytes(s)
        response.ContentLength64 <- int64 buffer.Length
        response.OutputStream.Write(buffer,0,buffer.Length)
        response.OutputStream.Close()
    member response.Reply(typ, buffer:byte[]) = 
        response.ContentLength64 <- int64 buffer.Length
        response.ContentType <- typ
        response.OutputStream.Write(buffer,0,buffer.Length)
        response.OutputStream.Close()

/// Simple HTTP server
type HttpServer private (url, root) =

    let contentTypes = 
        dict [ ".css", "text/css"; ".html", "text/html"; ".js", "text/javascript";
                ".gif", "image/gif"; ".png", "image/png"; ".jpg", "image/jpeg";
                ".mp3", "audio/mpeg"; ".wav", "audio/wav"; ".mpg", "video/mpeg" ]
    let tokenSource = new System.Threading.CancellationTokenSource()
  
    let agent = MailboxProcessor<System.Net.HttpListenerContext>.Start((fun inbox -> async { 
        while true do
        let! context = inbox.Receive()
        let s = context.Request.Url.LocalPath 

        // Handle an ordinary file request
        let file = root + (if s = "/" then "/index.html" else s)
        if System.IO.File.Exists(file) then 
            let ext = System.IO.Path.GetExtension(file).ToLower()
            let typ = contentTypes.[ext]
            context.Response.Reply(typ, System.IO.File.ReadAllBytes(file))
        else 
            context.Response.Reply(sprintf "File not found: %s" file) }), tokenSource.Token)

    let server = async { 
        use listener = new System.Net.HttpListener()
        listener.Prefixes.Add(url)
        listener.Start()
        while true do 
        let! context = listener.AsyncGetContext()
        agent.Post(context) }

    do Async.Start(server, cancellationToken = tokenSource.Token)

    /// Stops the HTTP server and releases the TCP connection
    member x.Stop() = tokenSource.Cancel()

    /// Starts new HTTP server on the specified URL. The specified
    /// function represents computation running inside the agent.
    static member Start(url, root) = 
        new HttpServer(url, root)

let url = sprintf "http://localhost:%d/" port
let server = HttpServer.Start(url, __SOURCE_DIRECTORY__)


