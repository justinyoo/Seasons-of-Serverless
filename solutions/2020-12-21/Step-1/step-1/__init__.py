# This function is not intended to be invoked directly. Instead it will be
# triggered by an HTTP starter function.

import logging
from datetime import timedelta

import azure.durable_functions as df


def orchestrator_function(context: df.DurableOrchestrationContext):
    logging.debug(context.get_input())

    req_data = dict(context.get_input())
    TIME_TO_SOAK = req_data['timeToSoakInMinutes']
    CALLBACK_URL = req_data['callbackUrl']

    is_soaked = False
    is_first = True
    
    while not is_soaked:
        if is_first:
            # First time to soak(or slice)
            delay_min = context.current_utc_datetime + timedelta(minutes=TIME_TO_SOAK)
            is_first = False
        else:
            # Not soaked yet So must check after 1 min.
            delay_min = context.current_utc_datetime + timedelta(minutes=1)

        yield context.create_timer(delay_min)
        result = yield context.call_activity('SoakSlice', CALLBACK_URL)

        is_soaked = result

        logging.info("Instance {} status : {}".format(context.instance_id, result))

    return True

main = df.Orchestrator.create(orchestrator_function)
