const BASE_URL = "http://localhost:5000/api";

export function getToken(): string | null {
  if (typeof window !== "undefined") {
    return localStorage.getItem("bf_token");
  }
  return null;
}

export function setToken(token: string) {
  if (typeof window !== "undefined") {
    localStorage.setItem("bf_token", token);
  }
}

export function clearToken() {
  if (typeof window !== "undefined") {
    localStorage.removeItem("bf_token");
    localStorage.removeItem("bf_username");
    localStorage.removeItem("bf_role");
  }
}

export function getSessionUser(): { username: string; role: string } | null {
  if (typeof window !== "undefined") {
    const username = localStorage.getItem("bf_username");
    const role = localStorage.getItem("bf_role");
    if (username && role) {
      return { username, role };
    }
  }
  return null;
}

async function request<T>(
  endpoint: string,
  options: RequestInit = {}
): Promise<T> {
  const token = getToken();
  
  const headers = new Headers(options.headers || {});
  if (token) {
    headers.set("Authorization", `Bearer ${token}`);
  }
  
  // Set JSON content-type automatically unless it is a FormData payload (e.g. file upload)
  if (!(options.body instanceof FormData) && !headers.has("Content-Type")) {
    headers.set("Content-Type", "application/json");
  }

  const response = await fetch(`${BASE_URL}${endpoint}`, {
    ...options,
    headers,
  });

  if (response.status === 204) {
    return {} as T;
  }

  const data = await response.json();

  if (!response.ok) {
    // Return validation or domain exception logs if available
    const errorDetails = data.errors || [data.detail || data.title || "An error occurred"];
    throw new Error(errorDetails.join(" | "));
  }

  return data as T;
}

export const api = {
  auth: {
    async login(username: string, role: string): Promise<string> {
      const response = await request<{ token: string }>("/auth/token", {
        method: "POST",
        body: JSON.stringify({ username, role }),
      });
      setToken(response.token);
      localStorage.setItem("bf_username", username);
      localStorage.setItem("bf_role", role);
      return response.token;
    },
  },
  
  accounts: {
    async getAll(page = 1, size = 20): Promise<{ items: any[]; totalCount: number }> {
      return request(`/accounts?pageNumber=${page}&pageSize=${size}`);
    },
    async getById(id: string): Promise<any> {
      return request(`/accounts/${id}`);
    },
    async create(accountCode: string, name: string, type: number, description?: string): Promise<any> {
      return request("/accounts", {
        method: "POST",
        body: JSON.stringify({ accountCode, name, type, description }),
      });
    },
  },

  journalEntries: {
    async getAll(page = 1, size = 15): Promise<{ items: any[]; totalCount: number }> {
      return request(`/journal-entries?pageNumber=${page}&pageSize=${size}`);
    },
    async getById(id: string): Promise<any> {
      return request(`/journal-entries/${id}`);
    },
    async create(referenceNumber: string, transactionDate: string, description: string, lines: any[]): Promise<any> {
      return request("/journal-entries", {
        method: "POST",
        body: JSON.stringify({ referenceNumber, transactionDate, description, lines }),
      });
    },
    async post(id: string): Promise<any> {
      return request(`/journal-entries/${id}/post`, {
        method: "POST",
      });
    },
  },

  invoices: {
    async getAll(page = 1, size = 10): Promise<{ items: any[]; totalCount: number }> {
      return request(`/invoices?pageNumber=${page}&pageSize=${size}`);
    },
    async getById(id: string): Promise<any> {
      return request(`/invoices/${id}`);
    },
    async upload(file: File): Promise<any> {
      const formData = new FormData();
      formData.append("file", file);
      return request("/invoices/upload", {
        method: "POST",
        body: formData,
      });
    },
    async create(data: any): Promise<any> {
      return request("/invoices", {
        method: "POST",
        body: JSON.stringify(data),
      });
    },
    async audit(id: string): Promise<any> {
      return request(`/invoices/${id}/audit`, {
        method: "POST",
      });
    },
    async approve(id: string, accountsPayableAccountId: string): Promise<any> {
      return request(`/invoices/${id}/approve`, {
        method: "POST",
        body: JSON.stringify({ accountsPayableAccountId }),
      });
    },
  },
};
