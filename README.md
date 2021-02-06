# midiot
Application for sending a list of midi commands when a midi message is received

Currently windows only - .netcore3.1 with WPF. If that comes to other platforms then it'd probably work.

The binary on the releases page is a self-contained .net executable (hence the size) so you shouldn't have to install any dependencies.

## File format
[label] command command command ... 

## Command types
- CHn - Set channel for subsequent commands to n
- PCn - Send program change 'n'
- CCn,v - Send control change number 'n' value 'v'

### Example file

\[intro\]CH2 PC12\
\[harmony\]PC0 CC34,1\
\[riffs\]CC34,0\
\[clean\]PC12\
\[riffs\]CC34,0\
\[clean\]PC12\
\[solo\]PC0 CC34,1\
\[riffs\]CC34,0

The midi channel only needs setting once until you want to change it
