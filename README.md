# amqpnetlite

Examples in the amqpnetlite folder are using the AMQP.Net Lite library (https://github.com/Azure/amqpnetlite), which supports only AMQP 1.0 protocol. To run the examples:
- Install the AMQP.Net Lite library
- Change the hostname / IP address, port number, paths to the certificates and queue names
- Run the examples

## BroadcastReceiver

This example connects to the AMQP broker, opens a consumer to the broadcast queue and starts consuming the broadcasts.

## RequestResponse

This example connects to the broker,connects to the broker, sends a request message and wait for a response, which should be sent by the Eurex system.

# Qpid.Messaging

Examples in Qpid.Messaging folder are using the C++ version Qpid Messaging API and its .NET binding. It supports both AMQP 1.0 and 0-10. To run the examples:
- Install Qpid Proton C library (only needed for AMQP 1.0 support)
- Install Qpid Messaging C++ API
- Install .NET binding for Qpid Messaging C++ API
- Change the hostname / IP address, port number, paths to the certificates and queue names
- Run the examples

## BroadcastReceiver.cs

This example connects to the AMQP broker using AMQP 0-10, opens a consumer to the broadcast queue and starts consuming the broadcasts.

## RequestRepsonse.cs

This example connects to the AMQP broker using AMQP 0-10, sends a request message and wait for a response, which should be sent by the Eurex system.

## 10_BroadcastReceiver.cs

This example connects to the AMQP broker using AMQP 1.0, opens a consumer to the broadcast queue and starts consuming the broadcasts.

## 10_RequestRepsonse.cs

This example connects to the AMQP broker using AMQP 1.0, sends a request message and wait for a response, which should be sent by the Eurex system.

# Documentation

More details about .NET APIs and code examples can be found in the Volume B of Eurex Clearing Messaging Interfaces documentation on http://www.eurexclearing.com/clearing-en/technology/eurex-release14/system-documentation/system-documentation/861464?frag=861450
