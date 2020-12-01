/*
 * This function is not intended to be invoked directly. Instead it will be
 * triggered by an orchestrator function.
 * 
 * Before running this sample, please:
 * - create a Durable orchestration function
 * - create a Durable HTTP starter function
 * - run 'npm install durable-functions' from the wwwroot folder of your
 *   function app in Kudu
 */

const request = require("request-promise-native");

module.exports = async function (context, callbackUrl) {

   
    var result = Math.random() <= 1;

    try {
        const data = await getCurrentConditions(callbackUrl);
    } catch (err) {
        context.log(`an error: ${err}`);
        throw new Error(err);
    }

    return `{"completed": ${result}}`;
}

async function getCurrentConditions(callbackUrl) {
    const options = {
        url: `${callbackUrl}`,
        method: 'POST',
        json: true,
        headers: {
            // "Accept": "application/json",
            "Content-Type": "application/json"
        },
        body: `{"completed": true}`
    };

    const body = await request(options);
    if (body.error) {
        throw body.error;
    } else if (body.response && body.response.error) {
        throw body.response.error;
    } else {
        return body.current_observation;
    }
}