// DO NOT MODIFY BELOW THIS LINE
// Generated by GiantTeam.Tools

export interface LoginInput {
    username?: string;
    password?: string;
    remainLoggedIn: boolean;
}

export interface LoginOutput {
    status: LoginStatus;
    message?: string;
}

export enum LoginStatus {
    Success = 200,
    Problem = 400,
}

export interface RegisterInput {
    name: string;
    email: string;
    username: string;
    password: string;
    passwordConfirmation: string;
}

export interface RegisterOutput {
    status: RegisterStatus;
    message?: string;
    userId?: string;
}

export enum RegisterStatus {
    Success = 200,
    Problem = 400,
}

export interface SessionOutput {
    status: SessionStatus;
    name?: string;
}

export enum SessionStatus {
    Anonymous = 0,
    Authenticated = 1,
}

export const postLogin = async (input: LoginInput): Promise<LoginOutput> => {
    const response = await fetch("/api/login", {
        method: "POST",
        headers: { "Content-Type": "application/json" },
        body: JSON.stringify(input)
    });
    return response.json();
}

export const postLogout = async () => {
    const response = await fetch("/api/logout", {
        method: "POST",
        headers: { "Content-Type": "application/json" },
    });
}

export const postRegister = async (input: RegisterInput): Promise<RegisterOutput> => {
    const response = await fetch("/api/register", {
        method: "POST",
        headers: { "Content-Type": "application/json" },
        body: JSON.stringify(input)
    });
    return response.json();
}

export const postSession = async (): Promise<SessionOutput> => {
    const response = await fetch("/api/session", {
        method: "POST",
        headers: { "Content-Type": "application/json" },
    });
    return response.json();
}