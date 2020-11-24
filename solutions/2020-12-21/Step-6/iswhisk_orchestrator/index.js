/*
 * This function is not intended to be invoked directly. Instead it will be
 * triggered by an HTTP starter function.
 * 
 * Before running this sample, please:
 * - create a Durable activity function (default name is "Hello")
 * - create a Durable HTTP starter function
 * - run 'npm install durable-functions' from the wwwroot folder of your 
 *    function app in Kudu
 */

const df = require("durable-functions");
const moment = require("moment");

module.exports = df.orchestrator(function*(context) {
    const whisk_waiting = 60;
    const wait_times = moment.utc(context.df.currentUtcDateTime).add(whisk_waiting, 's');

    context.df.setCustomStatus('{"completed" : false}');
    yield context.df.createTimer(wait_times.toDate());
    outputs.push(yield context.df.callActivity("whiskstatus", "completed"));
    context.df.setCustomStatus(outputs);
    return outputs;
});