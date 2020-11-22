# This function is not intended to be invoked directly. Instead it will be
# triggered by an HTTP starter function.

from datetime import timedelta
import logging
import json

import azure.durable_functions as df


def orchestrator_function(context: df.DurableOrchestrationContext):

    logging.debug(context.get_input())

    is_fried = False
    is_first = True
    
    try:
        fry_in_min = dict(context.get_input())['timeToStirFryInMinutes']
    except ValueError as e:
        raise e

    while not is_fried:
        if is_first:
            # First time to fry
            delay_time = context.current_utc_datetime + timedelta(seconds=fry_in_min)
            is_first = False

        else:
            # Not fried yet
            delay_time = context.current_utc_datetime + timedelta(minutes=1)

        yield context.create_timer(delay_time)
        result = yield context.call_activity('Fry')

        is_fried = result['completed']
        custom_status = {'completed': is_fried}
        context.set_custom_status(custom_status)

        logging.info("Instance {} status : {}".format(context.instance_id, is_fried))

    response_data = json.dumps({"completed": is_fried})
    return response_data

main = df.Orchestrator.create(orchestrator_function)
