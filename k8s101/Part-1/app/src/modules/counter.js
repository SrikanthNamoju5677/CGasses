/**
 * Newly added requires
 */
 var Register = require('prom-client').register;  
 var Counter = require('prom-client').Counter;  
 
 /**
  * A Prometheus counter that counts the invocations of the different HTTP verbs
  * e.g. a GET and a POST call will be counted as 2 different calls
  */
 module.exports.numOfRequests = numOfRequests = new Counter({  
     name: 'dockerapp_numOfRequests',
     help: 'Number of requests made',
     labelNames: ['method']
 });
 
 /**
  * A Prometheus counter that counts the invocations with different paths
  * e.g. /foo and /bar will be counted as 2 different paths
  */
 module.exports.pathsTaken = pathsTaken = new Counter({  
     name: 'dockerapp_pathsTaken',
     help: 'Paths taken in the app',
     labelNames: ['path']
 });

 
 /**
  * This function increments the counters that are executed on the request side of an invocation
  * Currently it increments the counters for numOfPaths and pathsTaken
  */
 module.exports.requestCounters = function (req, res, next) {  
     if (req.path != '/metrics') {
         numOfRequests.inc({ method: req.method });
         pathsTaken.inc({ path: req.path });
     }
     next();
 }