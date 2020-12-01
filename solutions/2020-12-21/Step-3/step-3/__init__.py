# This function is not intended to be invoked directly. Instead it will be
# triggered by an HTTP starter function.

from datetime import timedelta
import logging
import json

import azure.durable_functions as df


def orchestrator_function(context: df.DurableOrchestrationContext):

    logging.debug(context.get_input())

    req_data = dict(context.get_input())
    TIME_TO_STIR = req_data['timeToStirFryInMinutes']
    CALLBACK_URL = req_data['callbackUrl']

    is_fried = False
    is_first = True
    
    while not is_fried:
        if is_first:
            # First time to fry
            delay_time = context.current_utc_datetime + timedelta(minutes=TIME_TO_STIR)
            is_first = False

        else:
            # Not fried yet
            delay_time = context.current_utc_datetime + timedelta(minutes=1)

        yield context.create_timer(delay_time)
        result = yield context.call_activity('Fry', CALLBACK_URL)

        is_fried = result

        logging.info("Instance {} status : {}".format(context.instance_id, result))

    return True

main = df.Orchestrator.create(orchestrator_function)
