import BaseHTTPServer
import SocketServer

class InstructionsHolder:
    def __init__(self):
        self.value = 'These are instructions.'

INSTRUCTIONS = InstructionsHolder()

class RequestHandler(BaseHTTPServer.BaseHTTPRequestHandler):
    def do_GET(self):
        self.send_response(200)
        self.end_headers()
        self.wfile.write("""
<!doctype html>
<html>
<head>
<link href="https://fonts.googleapis.com/css?family=Roboto" rel="stylesheet">
<style>
html {
  width: 100%;
  height: 100%;
  margin: 0;
  background: black;
  color: white;
  font-family: 'Roboto', sans-serif;
  text-align: center;
}
body {
  width: 100%;
  height: 100%;
  margin: 0;
  padding: 24px;
  box-sizing: border-box;
  font-size: 36pt;
  display: flex;
  justify-content: center;
  align-items: center;
}
</style>
<script>
setInterval(function() {location = location;}, 1000);
</script>
</head>
<body>
""" + INSTRUCTIONS.value + """
</body>
</html>
""")

    def do_POST(self):
        content_len = int(self.headers.getheader('content-length', 0))
        INSTRUCTIONS.value = self.rfile.readline()
        INSTRUCTIONS.value = self.rfile.read(content_len)
        self.send_response(200)
        self.end_headers()
        self.wfile.write('')

if __name__  == '__main__':
    httpd = BaseHTTPServer.HTTPServer(("", 8000), RequestHandler)
    httpd.serve_forever()
