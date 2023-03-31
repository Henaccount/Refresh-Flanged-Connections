# Refresh-Flanged-Connections
Bolt calculation is done during flange connection creation, in some situations recalculation is needed.
To do this automatically you need to set up a new connection for Plant 3D (FL-FL, only size check, only gasket as connector).
Then you can run this script (command: RefreshFlangeConnections) with "JointType=Flanged,TargetJointType=dummy" as the config string. This will use the substitution menu to replace the standard FL-FL connection with the dummy connection. In a second run you switch back to the original connection with "TargetJointType=Flanged,JointType=dummy"
Now the bolt length should have been recalculated for all flanged connections and gasket sizes have updated without destroying the connection.
