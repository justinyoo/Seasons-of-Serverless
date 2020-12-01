# This function is not intended to be invoked directly. Instead it will be
# triggered by an orchestrator function.

from random import choice
import json
import logging
import requests

import azure.functions as func


def main(req: func.HttpRequest) -> dict:
    bool_data = [True, False]
    # pick_data = choice(bool_data)
    pick_data = True

    if pick_data:
        # Must execute when pick_data is True
        logging.info(req)
        requests.post(
            req,
            headers={"Content-Type": "application/json"},
            data=json.dumps({
                "completed": pick_data
            })
        )

    return pick_data
