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
                        }
                    },
                    "required": ["cause", "confidence", "reasoning"],
                    "additionalProperties": false
                }
            },
            "investigationSteps": {
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
            "warnings",
            "requiresHumanReview"
        ],
        "additionalProperties": false
    }
    """;
}