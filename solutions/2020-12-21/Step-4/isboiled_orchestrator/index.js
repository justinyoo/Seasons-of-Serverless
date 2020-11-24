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
    const boiled_wait_times = context.df.getInput()['timeToBoilInMinutes'];
    const boiled_wait_intervals = 60;
    const wait_times = moment.utc(context.df.currentUtcDateTime).add(boiled_wait_times, 'm');
    const expiryTime = moment.utc(context.df.currentUtcDateTime).add(60, 'm');

    context.df.setCustomStatus('{"boiled" : false}');
    yield context.df.createTimer(wait_times.toDate());

    while (moment.utc(context.df.currentUtcDateTime).isBefore(expiryTime)) 
    {
        const outputs = [];
        const boiledStatus = yield context.df.callActivity("boiledstatus", "boiled");
        const obj = JSON.parse(boiledStatus);

        if (obj.boiled === true) 
        {
            outputs.push(yield context.df.callActivity("boiledstatus", "boiled"));
            context.df.setCustomStatus(outputs);
            return outputs;
        }
        const nextCheck = moment.utc(context.df.currentUtcDateTime).add(boiled_wait_intervals, 's');
        outputs.push(yield context.df.callActivity("boiledstatus", "boiled"));
        context.df.setCustomStatus(outputs);
        yield context.df.createTimer(nextCheck.toDate());
    }
});