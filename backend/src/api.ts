import { type NextFunction, type Request, type Response } from "express";
const Openfort = require('@openfort/openfort-node').default;

const policy_id = 'pol_e7491b89-528e-40bb-b3c2-9d40afa4fefc';
const contract_id = 'con_26023265-1c7a-423e-87dd-0b3f8a2d20ef';
const chainId = 80002;

export class MintController {

    constructor() {
        this.run = this.run.bind(this);
    }

    async run(req: Request, res: Response, next: NextFunction) {

        const openfort = new Openfort(process.env.OPENFORT_SECRET_KEY);
        const accessToken = req.headers.authorization?.split(' ')[1];

        if (!accessToken) {
            return res.status(401).send({
                error: 'You must be signed in to view the protected content on this page.',
            });
        }

        const response = await openfort.iam.verifyOAuthToken({
            provider: 'firebase',
            token: accessToken,
            tokenType: 'idToken',
        });

        if (!response?.id) {
            return res.status(401).send({
                error: 'Invalid token or unable to verify user.',
            });
        }


        const playerId = response.id;

        const interaction_mint = {
            contract: contract_id,
            functionName: "mint",
            functionArgs: [playerId],
        };

        try {
            const transactionIntent = await openfort.transactionIntents.create({
                player: playerId,
                policy: policy_id,
                chainId,
                interactions: [interaction_mint],
            });

            console.log("transactionIntent", transactionIntent)
            return res.send({
                data: transactionIntent,
            });
        } catch (e: any) {
            console.log(e);
            return res.send({
                data: null,
            });
        }

    }
}
