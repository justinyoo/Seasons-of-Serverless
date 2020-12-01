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

    logging.info("TIME_TO_SOAK VALUE {}".format(TIME_TO_SOAK))
    if TIME_TO_SOAK is None:
        TIME_TO_SOAK = 0

    is_soaked = False
    is_first = True
    
    while not is_soaked:
        time = context.current_utc_datetime
        if is_first:
            # First time to soak(or slice)
            delay_min = time + timedelta(minutes=TIME_TO_SOAK)
            is_first = False
        else:
            # Not soaked yet So must check after 1 min.
            delay_min = time + timedelta(minutes=1)

        yield context.create_timer(delay_min)
        result = yield context.call_activity('SoakSlice', CALLBACK_URL)

        is_soaked = result

        logging.info("Instance {} status : {}\nCURR TIME: {} SOAK_T: {}\nTime accurate: {}"\
            .format(context.instance_id, is_soaked, 
                time, TIME_TO_SOAK, 
                    context.current_utc_datetime - (time + timedelta(minutes=TIME_TO_SOAK))))

    return True

main = df.Orchestrator.create(orchestrator_function)
