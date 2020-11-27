# This function is not intended to be invoked directly. Instead it will be
# triggered by an orchestrator function.

from random import choice

import azure.functions as func


def main(req: func.HttpRequest) -> dict:
    bool_data = [True, False]

    #random return if it is soaked(or sliced)
    return {"completed": choice(bool_data)}
