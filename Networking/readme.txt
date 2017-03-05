---------------------------
0.00 Contents
---------------------------
1.00  - Client
1.10  - Client Protocols
1.11 - Whois
1.12 - H9
1.13 - H0
1.14 - H1
1.2  - Client Responses
1.3  - Additional Arguments
2.00 - Server
2.10 - Server Requests
2.11 - Whois
2.12 - H9
2.13 - H0
2.14 - H1
2.20 - Server Reponses
2.21 - Whois
2.22 - H9
2.23 - H0
2.24 - H1



----------
1.00 Client
----------

--------------------
1.10 Client Protocols
--------------------

Sends a request to a server to find the location of a person by supplying a single argument, or setting the location by sending two arguements. Names cannot contain spaces, locations can.

Allows the sending of requests using 4 protocols depending on seperate identifiers.

----------
1.11 Whois
----------
Using no identifier results in the use of the whois protocol:

Finding:
<name>
Setting:
<name> <location>

-------------
1.12 Http/0.9
-------------
'/h9' denotes the Http/0.9 protocol:

Finding:
<name></h9>
Setting:
<name> <location></h9>

-------------
1.13 Http/1.0
-------------
'/h0' specifies Http/1.0 protocol:

Finding:
<name></h0>
Setting:
<name> <location></h0>

-------------
1.14 Http/1.1
-------------
'/h1' uses Http/1.1 protocol:

Finding:
<name></h1>
Setting:
<name> <location></h1>

The position of the request type within the arguement list is not required to be at the end, thus:

</h1> <name>
<name></h0><location>

are both valid request types.

--------------------
1.2 Client Responses
--------------------

In all cases, the client will respond with:

"<name> is <location>"
or
"ERROR: No entries found"

for search requests depending on if the person exists or not, and:

"<name> location changed to be <location>"

when setting values.

-------------------------
1.3 Additional Arguements
-------------------------

'/p' and '/h' can also be supplied to set the port number and hostname respectively:

<name></p><portNumber>
<name><location></h0></h><hostName>

Similar to the /h protocols they can be placed anywhere within the arguement list.



----------
2.00 Server
----------

The server does not save upon closing.
Maximum character length is 5000.
Times out if it doesnt recieve anything within 2500ms.

-------------------
2.10 Server Requests
-------------------
The server can handle requests from the same 4 protocols as the client can send. It recieves them as so:

----------
2.11 Whois
----------

Finding:
<name><CR><LF>

Setting:
<name> <location><CR><LF>

-------------
2.12 Http/0.9
-------------

Finding:
GET /<Name><CR><LF>

Setting:
PUT /<Name><CR><LF>
<CR><LF>
<location><CR><LF>

-------------
2.13 Http/1.0
-------------

Finding:
GET /?<Name> HTTP/1.0<CR><LF>
<optionalHeaderLines><CR><LF>

Setting:
POST /<Name> HTTP/1.0<CR><LF>
Content-Length: <locationLength><CR><LF>
<optionalHeaderLines><CR><LF>
<location>

-------------
2.14 Http/1.1
-------------

Finding:
GET /?name=<Name> HTTP/1.1<CR><LF>
Host: <hostName><CR><LF>
<optionalHeaderLines><CR><LF>

Setting:
POST / HTTP/1.1<CR><LF>
Host: <hostName><CR><LF>
Content-Length: <nameLength + locationLength + 15><CR><LF>
<optionalHeaderLines><CR><LF>
name=<name>&location=<location>

The +15 is to account for the name and location tags.

--------------------
2.20 Server responses
--------------------

----------
2.21 Whois
----------

Record found:
<location><CR><LF>

Record not found: 
ERROR: no entries found

Location changes:
OK<CR><LF>

-------------
2.22 Http/0.9
-------------

Record found:
HTTP/0.9 200 OK<CR><LF>
Content-Type: text/plain\<CR><LF>
<CR><LF>
<location><CR><LF>

Record not found: 
HTTP/0.9 404 Not Found<CR><LF>
Content-Type: text/plain<CR><LF>
<CR><LF>

Location changes:
HTTP/0.9 200 OK<CR><LF>
Content-Type: text/plain<CR><LF>
<CR><LF>

-------------
2.23 Http/1.0
-------------

Record found:
HTTP/1.0 200 OK<CR><LF>
Content-Type: text/plain\<CR><LF>
<CR><LF>
<location><CR><LF>

Record not found: 
HTTP/1.0 404 Not Found<CR><LF>
Content-Type: text/plain<CR><LF>
<CR><LF>

Location changes:
HTTP/1.0 200 OK<CR><LF>
Content-Type: text/plain<CR><LF>
<CR><LF>

-------------
2.24 Http/1.1
-------------

Record found:
HTTP/1.1 200 OK<CR><LF>
Content-Type: text/plain\<CR><LF>
<CR><LF>
<location><CR><LF>

Record not found: 
HTTP/1.1 404 Not Found<CR><LF>
Content-Type: text/plain<CR><LF>
<CR><LF>

Location changes:
HTTP/1.1 200 OK<CR><LF>
Content-Type: text/plain<CR><LF>
<CR><LF>