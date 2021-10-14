
# INSTRUCTIONS:
# 1) ENSURE YOU POPULATE THE LOCALS
# 2) ENSURE YOU REPLACE ALL INPUT PARAMETERS, THAT CURRENTLY STATE 'ENTER VALUE', WITH VALID VALUES
# 3) YOUR CODE WOULD NOT COMPILE IF STEP NUMBER 2 IS NOT PERFORMED!
# 4) ENSURE YOU CREATE A BUCKET FOR YOUR STATE FILE AND YOU ADD THE NAME BELOW - MAINTAINING THE STATE OF THE INFRASTRUCTURE YOU CREATE IS ESSENTIAL - FOR APIS, THE BUCKETS ALREADY EXIST
# 5) THE VALUES OF THE COMMON COMPONENTS THAT YOU WILL NEED ARE PROVIDED IN THE COMMENTS
# 6) IF ADDITIONAL RESOURCES ARE REQUIRED BY YOUR API, ADD THEM TO THIS FILE
# 7) ENSURE THIS FILE IS PLACED WITHIN A 'terraform' FOLDER LOCATED AT THE ROOT PROJECT DIRECTORY

terraform {
    required_providers {
        aws = {
            source  = "hashicorp/aws"
            version = "~> 3.0"
        }
    }
}

provider "aws" {
    region = "eu-west-2"
}

data "aws_caller_identity" "current" {}

data "aws_region" "current" {}

locals {
    parameter_store = "arn:aws:ssm:${data.aws_region.current.name}:${data.aws_caller_identity.current.account_id}:parameter"
}

terraform {
  backend "s3" {
    bucket  = "terraform-state-housing-development"
    encrypt = true
    region  = "eu-west-2"
    key     = "services/base-listener/state"
  }
}

### TODO - Replace all instances of SOME-SOURCE-DOMAIN and THIS_DOMAIN (in lower case) with the domains required below.

### This is the parameter containing the arn of the topic to which we want to subscribe
### This will have been created by the service the generates the events in which we are interested
#
# data "aws_ssm_parameter" "SOME-SOURCE-DOMAIN_sns_topic_arn" {
#   name = "/sns-topic/development/SOME-SOURCE-DOMAIN/arn"
# }

### This is the definition of the dead letter queue used whem message processsing fails for a given message
#
# resource "aws_sqs_queue" "THIS_DOMAIN_dead_letter_queue" {
#   name                        = "THIS_DOMAINdeadletterqueue.fifo"
#   fifo_queue                  = true
#   content_based_deduplication = true
#   kms_master_key_id           = "alias/housing-development-cmk"
#   kms_data_key_reuse_period_seconds = 300
# }

### This is the queue  which will receive the events published to the topic listed above
### This is what the listener lambda function will get triggered by.
#
# resource "aws_sqs_queue" "THIS_DOMAIN_queue" {
#   name                        = "THIS_DOMAINqueue.fifo"
#   fifo_queue                  = true
#   content_based_deduplication = true
#   kms_master_key_id           = "alias/housing-development-cmk"           # This is a custom key
#   kms_data_key_reuse_period_seconds = 300
#   redrive_policy              = jsonencode({
#     deadLetterTargetArn = aws_sqs_queue.THIS_DOMAIN_dead_letter_queue.arn,
#     maxReceiveCount     = 3                                               # Messages that fail processing are retried twice before being moved to the dead letter queue
#   })
# }

### This is the AWS policy that allows the topic to forward an event to the queue declared above
# 
# resource "aws_sqs_queue_policy" "THIS_DOMAIN_queue_policy" {
#   queue_url = aws_sqs_queue.THIS_DOMAIN_queue.id
#   policy = <<POLICY
#   {
#       "Version": "2012-10-17",
#       "Id": "sqspolicy",
#       "Statement": [
#       {
#           "Sid": "First",
#           "Effect": "Allow",
#           "Principal": "*",
#           "Action": "sqs:SendMessage",
#           "Resource": "${aws_sqs_queue.THIS_DOMAIN_queue.arn}",
#           "Condition": {
#           "ArnEquals": {
#               "aws:SourceArn": "${data.aws_ssm_parameter.SOME-SOURCE-DOMAIN_sns_topic_arn.value}"
#           }
#           }
#       }
#       ]
#   }
#   POLICY
# }

### This is the subscription definition that tells the topic which queue to use
# 
# resource "aws_sns_topic_subscription" "THIS_DOMAIN_queue_subscribe_to_SOME-SOURCE-DOMAIN_sns" {
#   topic_arn = data.aws_ssm_parameter.SOME-SOURCE-DOMAIN_sns_topic_arn.value
#   protocol  = "sqs"
#   endpoint  = aws_sqs_queue.THIS_DOMAIN_queue.arn
#   raw_message_delivery = true
# }

### This creates an AWS parameter with arn of the queue that will then be used within the Serverless.yml
### to specify the queue that will trigger the lambda function.
# 
# resource "aws_ssm_parameter" "THIS_DOMAIN_sqs_queue_arn" {
#   name  = "/sqs-queue/development/THIS_DOMAIN/arn"
#   type  = "String"
#   value = aws_sqs_queue.THIS_DOMAIN_queue.arn
# }
