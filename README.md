# GRpcFileCache
 Why File Cache for Kubernetes?
 ------------------------------
Many a time simple Cache solution is needed. It is also found that rather than using service that is outside the cluster, if some on cluster cache can be used it minimises the network calls out cside luster there by limiting network boundry crossing. On some experienced based metrics it was also observed that using on cluster cache was EBS volumes or Azure disk. If we use a Descale of deployments after a fixed amount of time or traffic ( using descalers like KEDA or using a DevOps based solution) then the cache service can also done so. If not, either the cloud based backing service are always running or some sort of constraining needs to be written to shut them down.

What this project is about
---------------------------
The project has two solutions.<br/>
* Cache Service
* Cache Client.

<p> The Cache service is the solution that does the cache actvity. It is a .NET core gRPC based solution. It uses file based persistence.  To use a different volumes only the PVC needs to be changed. <br>
The cache client has the necessary client library and swagger enable ui for exposing the code externally via ingress or LB <br>
