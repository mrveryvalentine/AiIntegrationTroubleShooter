namespace AiIntegrationTroubleShooter.Api.AI;

public static class AIResponseSchema
{
    public static readonly string Schema = """
    {
        "type": "object",
        "properties": {
            "summary": {
                "type": "string"
            },
            "possibleCauses": {
                "type": "array",
                "items": {
                    "type": "object",
                    "properties": {
                        "cause": {
                            "type": "string"
                        },
                        "confidence": {
                            "type": "string",
                            "enum": ["Low", "Medium", "High"]
                        },
                        "reasoning": {
                            "type": "string"
                        },
                        "evidenceToConfirm": {
                            "type": "string"
                        }
                    },
                    "required": [
                        "cause",
                        "confidence",
                        "reasoning",
                        "evidenceToConfirm"
                    ],
                    "additionalProperties": false
                }
            },
            "investigationSteps": {
                "type": "array",
                "items": {
                    "type": "string"
                }
            },
            "immediateActions": {
                "type": "array",
                "items": {
                    "type": "string"
                }
            },
            "warnings": {
                "type": "array",
                "items": {
                    "type": "string"
                }
            },
            "requiresHumanReview": {
                "type": "boolean"
            }
        },
        "required": [
            "summary",
            "possibleCauses",
            "investigationSteps",
            "immediateActions",
            "warnings",
            "requiresHumanReview"
        ],
        "additionalProperties": false
    }
    """;
}