# Laporan Live Browser Testing — Layar Operasional (Gelombang 4, `FE-HRD-R45`)

| Item | Keterangan |
| --- | --- |
| Menu yang diuji | **Kotak Masuk Persetujuan Terpadu (V2)** (`S-A7-V2`, 6 rute), **Perjalanan Dinas & Biaya (SPPD)** (`S-D5`, 8 rute), **Layanan SDM & Meja Bantuan** (`S-D4`, 5 rute), **Manajemen Lembur** (`S-B3`, 7 rute) |
| Task roadmap | `FE-HRD-R45` — Verifikasi layar operasional |
| Tanggal uji | Rabu, 18 November 2026, pagi hingga 11.45 WIB |
| Penguji | AI Agent (Claude Code, skill `live-browser-testing`) dengan Google Chrome (Chromium) mode visual/headed |
| Lingkungan | Frontend `http://localhost:3000` (branch `HamzahV2`); backend `https://localhost:7184` (branch `MHamzah`, build scratch, `Runtime__Role=Web`) |
| Database | `QuilvianNewDevHamzah` |
| Akun | `superadmin@admin.com` (admin), `dewi.anggraeni@rsmmc.local` (pegawai non-admin), `mira.safitri@rsmmc.local` (pegawai penyetuju, punya profil pegawai) |
| Persetujuan pemilik | 18 Nov 2026: backend dinyalakan apa adanya; uji membaca; galat disimulasikan di browser; **satu penulisan yang dapat dipulihkan**: terbitkan surat kuasa Plh → muat ulang → cabut |

---

## 1. Ringkasan Eksekutif

**Kesimpulan: acceptance criteria "keputusan tetap tersimpan setelah halaman dimuat ulang" TERBLOKIR backend.** Satu-satunya penulisan yang diizinkan, yaitu menerbitkan surat kuasa Plh, ditolak server dengan HTTP 500. Penyebabnya bug di `ApprovalDelegationV2Service`: setiap pembuatan delegasi melanggar constraint basis data. Tidak ada data yang tersimpan. Seluruh 26 layar operasional termuat tanpa galat, dan kegagalan server tampil jujur.

Contoh di rumah sakit: Kepala Ruang ICU yang akan cuti tiga hari menunjuk Ketua Tim sebagai Plh. Setelah menekan **Terbitkan Surat Kuasa**, ia mendapat pesan teknis berbahasa Inggris "An error occurred while saving the entity changes…", dan surat kuasa tidak terbit. Selama cuti, permohonan cuti atau lembur bawahannya menumpuk di kotak masuk tanpa ada yang bisa menyetujui.

| Ringkasan | Jumlah |
| --- | ---: |
| Test case | 66 (26 admin + 26 non-admin + 8 simulasi galat + 5 langkah uji tulis delegasi + 1 keterlihatan menu non-admin) |
| ✅ Lulus | 63 |
| ❌ Gagal (blocker backend) | 1 (LBT-W4-OP-01, P0) |
| ⚠️ Lulus dengan temuan | 1 (keterlihatan menu, LBT-W4-OP-03) |
| ⏸️ Berhenti karena langkah sebelumnya gagal | 1 (muat ulang → cabut) |
| Temuan unik | 3 (P0: 1 · P1: 1 · P2: 1) |

**Yang sudah berjalan baik:**
- Admin: 26/26 layar termuat. Satu `404` (portal ESS untuk superadmin tanpa profil pegawai) tertangani dengan pesan yang jelas.
- Non-admin: 17 layar menolak dengan pesan "belum punya hak".
- Simulasi `500`/`403` pada Delegasi, Settlement SPPD, Tiket Layanan, dan Verifikasi Lembur: 8/8 pesan ramah, tanpa data contoh.
- Validasi formulir Plh: tombol **Terbitkan Surat Kuasa** nonaktif saat formulir kosong, dan alasan kurang dari 10 karakter ditolak dengan pesan "Alasan penunjukan minimal 10 karakter."

---

## 2. Alur Proses Bisnis yang Diuji

1. **Pemicu** — penyetuju (Mira Safitri) akan berhalangan dan membuka **Kotak Masuk Persetujuan Terpadu → Pendelegasian Wewenang (Plh)**.
2. **Isi formulir** — klik **+ Tunjuk Plh**, pilih pejabat pengganti (Amanda Fitriani), tetapkan masa berlaku 18–20 Nov 2026 dan cakupan "Semua jenis permohonan", lalu tulis alasan.
3. **Terbitkan** — klik **Terbitkan Surat Kuasa** sehingga frontend mengirim `POST /api/v1/hrd/workflow/delegations`.
4. **Bukti tersimpan** — muat ulang halaman; surat kuasa harus masih tampil di daftar aktif.
5. **Pemulihan** — klik **Cabut**, isi alasan, lalu muat ulang; surat kuasa harus pindah ke riwayat dengan status dicabut.
6. **Hasil sebenarnya** — langkah 3 ditolak server (`500`), sehingga langkah 4–5 tidak dijalankan.

## 3. Persiapan Lingkungan & Data

Sama dengan [laporan R42](laporan-live-testing-gelombang4-perencanaan-tenaga-kerja.md#3-persiapan-lingkungan--data). Alasan uji yang ditulis: "Uji Gelombang 4 HR: Plh sementara dua hari, akan dicabut kembali." Nilai ini dipakai untuk menemukan dan mencabut data bila sempat tersimpan.

## 4. Endpoint yang Diuji

Auth: cookie sesi JWT + `[AccessPermission]`.

### `[Tags("Corporate - HRD Workflow Delegations")]`

| Method | Path | Deskripsi | Request | Respons yang teramati |
| --- | --- | --- | --- | --- |
| GET | `/api/v1/hrd/workflow/delegations` | Daftar surat kuasa Plh | query filter aktif | `200` |
| POST | `/api/v1/hrd/workflow/delegations` | Terbitkan surat kuasa Plh | `{ delegateEmployeeId, startDate, endDate, delegatedRequestTypes: "ALL", delegationReason }` (`CreateDelegationModal.jsx:53-58`) | **`500`**: `{"message":"An error occurred while saving the entity changes. See the inner exception for details."}`; log server: `PostgresException 23514 ... violates check constraint "CK_TrxApprovalDelegation_WorkflowScope"` |
| PUT | `/api/v1/hrd/workflow/delegations/{id}/revoke` | Cabut surat kuasa | `{ revokeReason }` | Tidak dijalankan (tidak ada data yang perlu dicabut) |

### `[Tags("Corporate - HRD Business Travel Settlements")]`, `[Tags("HR Services - Tickets & Queue")]`, `[Tags("HR Overtime - Biometric Verification & Discrepancies")]`

| Method | Path | Deskripsi | Respons yang teramati |
| --- | --- | --- | --- |
| GET | `/api/v1/hrd/travel/settlements` | Antrean settlement SPPD | `200`; simulasi → pesan ramah |
| GET | `/api/v1/corporate/hrd/services/tickets` | Antrean tiket layanan SDM | `200`; simulasi → pesan ramah |
| GET | `/api/v1/corporate/human-resource/overtime/verifications` | Verifikasi lembur biometrik | `200`; simulasi → pesan ramah |

## 5. Rincian Test Case

### P0 — Critical / Fatal Functional

| ID | Skenario | Hasil |
| --- | --- | --- |
| OP-P0-01 s.d. -26 | 26 layar dibuka admin | ✅ 26/26 |
| OP-P0-27 s.d. -52 | 26 layar dibuka non-admin | ✅ tanpa crash; 17 menolak dengan pesan tanpa hak |
| OP-P0-53 | Terbitkan surat kuasa Plh dengan formulir lengkap | ❌ HTTP 500 (LBT-W4-OP-01). Alur berhenti sesuai Fatal Gate; bukti `R45-tulis/04-setelah-terbit.png` |
| OP-P0-54 | Muat ulang → surat kuasa masih ada → cabut | ⏸️ Tidak dijalankan karena OP-P0-53 gagal |

### P1 — Business Compliance

| ID | Skenario | Hasil |
| --- | --- | --- |
| OP-P1-01 | Delegasi, Settlement, Tiket, Verifikasi Lembur saat `500` dan `403` tiruan | ✅ 8/8 pesan ramah |
| OP-P1-02 | Menu operasional untuk non-admin | ⚠️ Sebagian butir tanpa `requiredPermission` tetap terlihat (V2 3/6, SPPD 4/8, Layanan SDM 2/5, Lembur 4/7), lalu ditolak saat dibuka (LBT-W4-OP-03) |

### P2 & P3 — Validasi formulir Plh

| ID | Skenario | Hasil |
| --- | --- | --- |
| OP-P2-01 | Formulir kosong → tombol terbitkan nonaktif | ✅ |
| OP-P2-02 | Alasan "Cuti" (4 karakter) → pesan minimal 10 karakter, tombol nonaktif | ✅ |
| OP-P2-03 | Formulir lengkap → tombol aktif | ✅ |

## 6. Daftar Temuan & Rekomendasi

### P0 — Fatal

| ID | Temuan | Contoh dampak | Rekomendasi (backend) |
| --- | --- | --- | --- |
| LBT-W4-OP-01 | `ApprovalDelegationV2Service` membuat `TrxApprovalDelegation` dengan `DelegatedRequestTypes = "ALL"` tanpa mengisi `AppliesToAllWorkflows`, `WorkflowDefinitionId`, atau `ScopeDefinitionJson`. Constraint `CK_TrxApprovalDelegation_WorkflowScope` (migration `20260730050557_initializeTrxWorkflowApproverAssignments`) mewajibkan salah satunya, sehingga **semua** penerbitan surat kuasa Plh gagal. | Tidak ada pejabat yang bisa menunjuk Plh dari layar; persetujuan macet selama pejabat cuti | Isi `AppliesToAllWorkflows = true` bila cakupan "ALL", atau `WorkflowDefinitionId`/`ScopeDefinitionJson` sesuai cakupan, lalu tambahkan test integrasi pembuatan delegasi |

### P2 — Fungsional

| ID | Temuan | Contoh dampak | Rekomendasi |
| --- | --- | --- | --- |
| LBT-W4-OP-02 | Respons 500 meneruskan pesan exception EF berbahasa Inggris ke pengguna | Pengguna melihat "See the inner exception for details" dan tidak tahu harus berbuat apa | Backend: tangkap `DbUpdateException` dan kembalikan pesan Bahasa Indonesia tanpa detail teknis |

### P1 — Kepatuhan (navigasi)

| ID | Temuan | Rekomendasi |
| --- | --- | --- |
| LBT-W4-OP-03 | Butir menu operasional tanpa `requiredPermission` terlihat oleh semua pegawai, padahal halamannya menolak | Tambahkan `requiredPermission` yang sesuai di `menu-items.jsx`; butir layanan mandiri (portal ESS, pengajuan SPPD/lembur sendiri) tetap terbuka |

## 7. Tidak Dapat Diuji / Tertahan

| Hal | Alasan |
| --- | --- |
| AC "keputusan tetap tersimpan setelah dimuat ulang" untuk delegasi | ⛔ LBT-W4-OP-01 |
| Keputusan lain: setujui/tolak di kotak masuk V2, settlement SPPD, resolusi tiket, verifikasi lembur | Penulisan di luar izin Gelombang 4 (hanya satu delegasi). Risiko roadmap: "butuh data uji". |

## 8. Data Uji yang Tertinggal di `QuilvianNewDevHamzah`

Tidak ada. Pemeriksaan baca-saja pada 18 Nov 2026 pukul 11.40 WIB: `TrxApprovalDelegation` dengan alasan "Uji Gelombang 4%" = **0 baris**.

## 9. Lampiran Teknis

| Berkas | Isi |
| --- | --- |
| `test-with-agy/scripts/hrw4-1118-r45-delegation.mjs` | Uji validasi + terbitkan → muat ulang → cabut (berhenti bila penerbitan ditolak) |
| `test-with-agy/screenshots/hrw4-1118/R45-tulis/` | 01-daftar-awal s.d. 04-setelah-terbit |
| `test-with-agy/screenshots/hrw4-1118/R45-admin/`, `R45-nonadmin/`, `R45-kegagalan-server/` | Sapu dan simulasi |

## 10. Tindak Lanjut

1. **Backend (prioritas tertinggi):** perbaiki LBT-W4-OP-01, lalu ulangi uji tulis ini (`node test-with-agy/scripts/hrw4-1118-r45-delegation.mjs`, akun penyetuju).
2. Backend: pesan galat 500 tanpa detail EF (LBT-W4-OP-02).
3. Frontend: `requiredPermission` menu operasional (LBT-W4-OP-03).
4. Setelah pemilik mengizinkan data uji: uji keputusan V2, SPPD, tiket, dan lembur dengan muat ulang.
