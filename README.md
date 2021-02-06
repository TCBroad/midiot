# midiot
Application for sending a list of midi commands when a midi message is received

## File format
[label] command command command ... 

## Command types
- CH - Set channel for subsequent commands (you don't have to add this to every command list)
- PC<n> - Send program change 'n'
- CC<n,v> - Send control change number 'n' value 'v'

### Example file
[intro]CH2 PC12
[harmony]PC0 CC34,1
[riffs]CC34,0
[clean]PC12
[riffs]CC34,0
[clean]PC12
[solo]PC0 CC34,1
[riffs]CC34,0

The midi channel only needs setting once until you want to change it
