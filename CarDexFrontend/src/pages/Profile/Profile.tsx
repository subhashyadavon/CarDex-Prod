import React, { useState, useEffect } from "react";
import { useAuth } from "../../hooks/useAuth";
import { userService } from "../../services/userService";
import styles from "./Profile.module.css";
import { NavItem } from "../../components/Header/Header";

interface ProfileProps {
    isEditingInitial?: boolean;
    onModeChange?: (mode: NavItem) => void;
}

const Profile: React.FC<ProfileProps> = ({ isEditingInitial = false, onModeChange }) => {
    const { user, updateUserCurrency } = useAuth();
    const [isEditing, setIsEditing] = useState(isEditingInitial);
    const [username, setUsername] = useState(user?.username || "");
    const [password, setPassword] = useState("");
    const [error, setError] = useState("");
    const [success, setSuccess] = useState("");
    const [isLoading, setIsLoading] = useState(false);

    useEffect(() => {
        if (user) {
            setUsername(user.username);
        }
    }, [user]);

    // Sync isEditing state if isEditingInitial prop changes
    useEffect(() => {
        setIsEditing(isEditingInitial);
    }, [isEditingInitial]);

    const handleSave = async (e: React.FormEvent) => {
        e.preventDefault();
        if (!user) return;

        setError("");
        setSuccess("");
        setIsLoading(true);

        try {
            const updates: { username?: string; password?: string } = {};
            if (username !== user.username) updates.username = username;
            if (password) updates.password = password;

            if (Object.keys(updates).length === 0) {
                setIsEditing(false);
                if (onModeChange) onModeChange("PROFILE");
                setIsLoading(false);
                return;
            }

            const updatedUser = await userService.updateProfile(user.id, updates);

            // Update local storage and context if username changed
            if (updates.username) {
                const storedUser = JSON.parse(localStorage.getItem("user") || "{}");
                const newUser = { ...storedUser, username: updatedUser.username };
                localStorage.setItem("user", JSON.stringify(newUser));
            }

            setSuccess("Profile updated successfully!");
            setIsEditing(false);
            if (onModeChange) onModeChange("PROFILE");
            setPassword("");
        } catch (err: any) {
            setError(err.response?.data?.message || "Failed to update profile");
        } finally {
            setIsLoading(false);
        }
    };

    if (!user) return <div>Loading...</div>;

    return (
        <div className={styles.profileContainer}>
            <h1 className={styles.title}>User Profile</h1>

            <div className={styles.profileCard}>
                {!isEditing ? (
                    <>
                        <div className={styles.section}>
                            <h2 className={styles.sectionTitle}>Account Information</h2>
                            <div className={styles.infoGrid}>
                                <div className={styles.infoItem}>
                                    <span className={styles.label}>Username</span>
                                    <span className={styles.value}>{user.username}</span>
                                </div>
                                <div className={styles.infoItem}>
                                    <span className={styles.label}>Currency</span>
                                    <span className={styles.value}>🪙 {user.currency.toLocaleString()}</span>
                                </div>
                            </div>
                        </div>
                    </>
                ) : (
                    <form onSubmit={handleSave}>
                        <div className={styles.section}>
                            <h2 className={styles.sectionTitle}>Edit Account Details</h2>

                            <div className={styles.formGroup}>
                                <label className={styles.formLabel}>Username</label>
                                <input
                                    type="text"
                                    className={styles.input}
                                    value={username}
                                    onChange={(e) => setUsername(e.target.value)}
                                    placeholder="New username"
                                />
                            </div>

                            <div className={styles.formGroup}>
                                <label className={styles.formLabel}>New Password (leave blank to keep current)</label>
                                <input
                                    type="password"
                                    className={styles.input}
                                    value={password}
                                    onChange={(e) => setPassword(e.target.value)}
                                    placeholder="New password"
                                />
                            </div>

                            {error && <div className={styles.error}>{error}</div>}
                            {success && <div className={styles.success}>{success}</div>}
                        </div>

                        <div className={styles.buttonGroup}>
                            <button
                                type="submit"
                                className={styles.saveButton}
                                disabled={isLoading}
                            >
                                {isLoading ? "Saving..." : "Save Changes"}
                            </button>
                            <button
                                type="button"
                                className={styles.cancelButton}
                                onClick={() => {
                                    setIsEditing(false);
                                    if (onModeChange) onModeChange("PROFILE");
                                    setUsername(user.username);
                                    setPassword("");
                                }}
                            >
                                Cancel
                            </button>
                        </div>
                    </form>
                )}
            </div>
        </div>
    );
};

export default Profile;
