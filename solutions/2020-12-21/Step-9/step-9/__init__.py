import json
import logging
import os
from random import choice

import azure.functions as func
from azure.storage.blob import ContainerClient
from dotenv import load_dotenv


load_dotenv(verbose=True)

def main(req: func.HttpRequest) -> func.HttpResponse:

    logging.info(req.get_json())

    try:
        is_pepper_include = req.get_json()['pepper']
    except:
        return func.HttpResponse("Write down whether to include pepper or not", status_code=400)


    try:
        if is_pepper_include:
            url = os.environ["PEPPER_CONTAINER"]
            container = ContainerClient.from_container_url(url)
        else:
            url = os.environ["NO_PEPPER_CONTAINER"]
            container = ContainerClient.from_container_url(url)

        blob_list = list(container.list_blobs())
        blob = choice(blob_list)

    except Exception as e:
        logging.info("Err: {}".format(e))
        return func.HttpResponse(str(e), status_code=500)

    image_url = blob.name

    return_data = {
        "tteokgukImageUrl": url + image_url
    }

    return func.HttpResponse(json.dumps(return_data), 
        headers={"Content-Type": "application/json"}, status_code=200)
