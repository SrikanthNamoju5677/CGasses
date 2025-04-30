const express = require('express');
const app = express();
const db = require('./persistence');
const getItems = require('./routes/getItems');
const addItem = require('./routes/addItem');
const updateItem = require('./routes/updateItem');
const deleteItem = require('./routes/deleteItem');

const promBundle = require("express-prom-bundle");
const { Histogram } = require('prom-client');

var Prometheus = require('./modules/counter');

app.use(express.json());
app.use(express.static(__dirname + '/static'));

//start using Prometheus
app.use(Prometheus.requestCounters);  

// Add the options to the prometheus middleware most option are for http_request_duration_seconds histogram metric
const metricsMiddleware = promBundle({
    includeMethod: true,
    includePath: true,
    includeStatusCode: true,
    includeUp: true,
    customLabels: {project_name: 'dockerapp', project_type: 'test_metrics_labels'},
    metricsType: Histogram,
    promClient: {
        collectDefaultMetrics: {
        }
      }
});


// add the prometheus middleware to all routes
app.use(metricsMiddleware)

// default endpoint
app.get("/",(req,res) => res.json({
    "GET /": "All Routes",
    "GET /metrics": "Metrics data"
}));


//The app itself
app.get('/items', getItems);
app.post('/items', addItem);
app.put('/items/:id', updateItem);
app.delete('/items/:id', deleteItem);


db.init().then(() => {
    app.listen(4000, () => console.log('Listening on port 4000'));
}).catch((err) => {
    console.error(err);
    process.exit(1);
});

const gracefulShutdown = () => {
    db.teardown()
        .catch(() => {})
        .then(() => process.exit());
};

app.listen(8080, function () {    
    console.log('Listening at http://app.apps:8080');     
    console.log('Listening at http://localhost:8080'); 
     
  });

process.on('SIGINT', gracefulShutdown);
process.on('SIGTERM', gracefulShutdown);
process.on('SIGUSR2', gracefulShutdown); // Sent by nodemon