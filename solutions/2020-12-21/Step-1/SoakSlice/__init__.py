# This function is not intended to be invoked directly. Instead it will be
# triggered by an orchestrator function.

from random import choice
import json
import logging
import requests

import azure.functions as func


def main(req: func.HttpRequest) -> dict:
    bool_data = [True, False]
    pick_data = choice(bool_data)

    if pick_data:
        # Must execute when pick_data is True
        res = requests.post(
            req,
            headers={"Content-Type": "application/json"},
            data=json.dumps({
                "completed": pick_data
            })
        )

        logging.info("Callback_url: {} res: {} {}"\
            .format(req, res.status_code, res.content))


    return pick_data
