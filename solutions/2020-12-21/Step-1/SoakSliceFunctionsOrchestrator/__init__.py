# This function is not intended to be invoked directly. Instead it will be
# triggered by an HTTP starter function.

from datetime import timedelta
import logging
import json

import azure.functions as func
import azure.durable_functions as df


def orchestrator_function(context: df.DurableOrchestrationContext):
    logging.debug(context.get_input())

    is_soaked = False
    is_first = True
    
    while not is_soaked:
        if is_first:
            # First time to soak(or slice)
            delay_sec_n = context.current_utc_datetime + timedelta(seconds=dict(context.get_input())['timeToSoakInMinutes'])
            yield context.create_timer(delay_sec_n)
            is_first = False
        else:
            # Not soaked yet So must check after 1 min.
            delay_min_1 = context.current_utc_datetime + timedelta(minutes=1)
            yield context.create_timer(delay_min_1)

        result = yield context.call_activity('SoakSlice')

        is_soaked = result['completed']
        custom_status = {'completed': is_soaked}
        context.set_custom_status(custom_status)

        logging.info("Instance {} status : {}".format(context.instance_id, is_soaked))

    response_data = json.dumps({"completed": is_soaked})
    return response_data

main = df.Orchestrator.create(orchestrator_function)
