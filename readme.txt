# fqueue

* A simple .net core queue

Opens an endpoint on port 1024
Waits for tcp connections:
Protocol:
    enqueue:
        -- Open Connection
        -- SEND
        {
            "type": "push",
            "queue": "<queuename>",
            "data": "whatever"
        }\n\n
        -- Close Connection

    dequeue:
        -- Open Connection
        -- SEND
        {
            "type": "push",
            "queue": "<queuename>"
        }\n\n
        -- RECV
        <data>
        -- Close Connection

Console commands:
 * push <queue> <data>  -- push an item onto the queue
 * pop <queue>          -- pop an item from the queue
 * list                 -- list the queues
 * exit                 -- exit and shut down

Todo:
   * Want this to be more distributed
   * Needs to handle bad input
   * Current max items is a hard 1,000,000

Config:
   * Self explanitory from the example.
   * Only config property is which queues to create
