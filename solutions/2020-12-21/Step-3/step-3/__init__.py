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

    logging.info("TIME_TO_STIR VALUE {}".format(TIME_TO_STIR))
    if TIME_TO_STIR is None:
        TIME_TO_STIR = 0

    is_fried = False
    is_first = True
    
    while not is_fried:
        time = context.current_utc_datetime
        if is_first:
            # First time to fry
            delay_time = time + timedelta(minutes=TIME_TO_STIR)
            is_first = False

        else:
            # Not fried yet
            delay_time = time + timedelta(minutes=1)

        yield context.create_timer(delay_time)
        result = yield context.call_activity('Fry', CALLBACK_URL)

        is_fried = result

        logging.info("Instance {} status : {}\nCURR TIME: {} STIR_T: {}\nTime accurate: {}"\
            .format(context.instance_id, is_fried, 
                time, TIME_TO_STIR, 
                    context.current_utc_datetime - (time + timedelta(minutes=TIME_TO_STIR))))

    return True

main = df.Orchestrator.create(orchestrator_function)
