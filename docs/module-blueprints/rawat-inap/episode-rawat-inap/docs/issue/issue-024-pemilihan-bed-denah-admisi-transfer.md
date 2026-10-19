# ISSUE-024 — Pemilihan Tempat Tidur Jalur Admisi Transfer Pasien Belum Menggunakan Denah

```yaml
issue_id: ISSUE-EPS-024
module_id: rawat-inap
submodule: episode-rawat-inap
layar: "Admisi Rawat Inap — Langkah 5 Pilih Bed Jalur Transfer IGD/Poli (FE-INP-03)"
sumber_laporan: "Laporan pengguna 17-10-2026: pada pilih bed di menu admisi tranfer pasien belum bisa tampil bed denah ya, gunakan bed denah untuk pemilihan, tolong anda analisis kenapa masalah itu bisa terjadi lalu berikan plaaning ya"
tanggal_issue: "2026-10-17"
status: SELESAI
tanggal_selesai: "2026-10-17"
keparahan_tertinggi: Medium
source_sha_backend: "c2bc563ed9332f08fa95b9fe35786fea7a647279 (MHamzah)"
source_sha_frontend: "15ae73984ad26a408e3038566d7e1d1533fe10c3 (HamzahV2)"
rencana_perbaikan: ../plan-repair/plan-repair-024-pemilihan-bed-denah-admisi-transfer.md
```

---

## 1. Ringkasan

Laporan pengguna menyoroti bahwa pada layar **Admisi Rawat Inap** alur **Transfer Pasien IGD/Poliklinik** (`/health-services/inpatient-management/admission?mode=transfer`), **Langkah 5 (Pilih Bed)** belum menampilkan visual denah ruangan interaktif (*floor plan*). Petugas saat ini hanya disajikan satu kotak pilihan dropdown biasa (*select dropdown*) bertuliskan *"Tempat Tidur \*"* dengan daftar teks bed. Hal ini kontras dengan alur Admisi Pasien Baru/Lama dan Admisi Bayi (`FE-RWI-248`) serta alur Pindah Kamar rawat inap (`FE-RWI-249`) yang telah berhasil mengadopsi peta denah visual per lantai.

Akar masalahnya terbagi menjadi dua lapisan:
1. **Lapisan Frontend:** Pada komponen [inpatient-admission-bed-step.jsx](file:///c:/Users/Asoiii/Documents/Quilvian/SourceCode/QuilvianFinal/QuilvianSystemFrontendDev/src/components/view/health-services/inpatient-management/inpatient-admission-bed-step.jsx#L128-L184) dan hook [use-inpatient-admission-bed.jsx](file:///c:/Users/Asoiii/Documents/Quilvian/SourceCode/QuilvianFinal/QuilvianSystemFrontendDev/src/lib/hooks/health-services/inpatient-management/use-inpatient-admission-bed.jsx#L122), variabel penentu mode `usesAdmissionFloorPlan` secara sengaja dikunci menjadi `false` saat alur mendeteksi mode transfer (`Boolean(admission && !transfer)`). Hal ini menyebabkan antarmuka secara otomatis beralih ke blok *fallback* dropdown konvensional.
2. **Lapisan Kontrak & Backend:** Pada alur transfer dari IGD/Poli, episode rawat inap belum ada di database saat berada di Langkah 5 karena transaksi admisi transfer dieksekusi secara atomik pada Langkah 6 melalui endpoint `POST /admission-transfers/admit`. Di sisi lain, endpoint backend `GET /bed-occupancies/floor-plan` saat ini mewajibkan parameter `EpisodeId` yang harus berstatus `Draft` (jika `purpose=Admission`) atau `Admitted` (jika `purpose=Transfer` internal). Ketiadaan `EpisodeId` pada tahap pemilihan bed transfer menjadi alasan pengembang terdahulu menahan integrasi denah pada alur ini saat pengerjaan task `FE-RWI-248`.

Temuan ini diklasifikasikan sebagai `DESIGN_CHANGE` dan `GAP_INTEGRASI` dengan tingkat keparahan `Medium` (alur operasional tetap dapat diselesaikan lewat dropdown, namun pengalaman pengguna terpecah dan tidak konsisten dengan standarisasi denah rumah sakit).

---

## 2. Laporan Asli

| No. Lap | Kalimat Pelapor | Lampiran |
| :---: | :--- | :--- |
| 1 | "pada pilih bed di menu admisi tranfer pasien belum bisa tampil bed denah ya" | Screenshot layar Admisi Rawat Inap — Langkah 5 dari 9: Pilih Bed, menampilkan dropdown "Pilih bed yang tersedia" dan alert penempatan server. |
| 2 | "gunakan bed denah untuk pemilihan" | (Sama dengan di atas) |
| 3 | "tolong anda analisis kenapa masalah itu bisa terjadi lalu berikan plaaning ya" | (Sama dengan di atas) |

---

## 3. Ringkasan Temuan

| ID | No. Lap | Judul Temuan | Jenis | Area | Keparahan | Status Bukti | ID Perbaikan |
| :--- | :---: | :--- | :--- | :---: | :---: | :---: | :---: |
| `ISS-EPS-024-01` | 1, 2 | Langkah Pilih Bed admisi transfer masih memakai dropdown teks dan mengunci denah | `DESIGN_CHANGE`, `GAP_INTEGRASI` | Frontend | `Medium` | `SUDAH-VERIFIKASI` | `FIX-EPS-024-01` |
| `ISS-EPS-024-02` | 3 | Kontrak `GET /floor-plan` backend mewajibkan `EpisodeId`, padahal alur transfer belum memiliki episode di langkah 5 | `CONTRACT_MISMATCH` | Backend / Frontend Adapter | `High` | `SUDAH-VERIFIKASI` | `FIX-EPS-024-02` |

---

## 4. Rincian per Temuan

### ISS-EPS-024-01 — Langkah Pilih Bed admisi transfer masih memakai dropdown teks dan mengunci denah

| Field | Nilai |
| :--- | :--- |
| **No. laporan** | 1, 2 |
| **Jenis** | `DESIGN_CHANGE`, `GAP_INTEGRASI` |
| **Area** | Frontend |
| **Keparahan** | `Medium` |
| **Status bukti** | `SUDAH-VERIFIKASI` di source code frontend |
| **Perbaikan** | `FIX-EPS-024-01` |

**Apa yang terjadi.**  
Saat petugas admisi membuka menu Admisi Rawat Inap jalur transfer (pasien rujukan rawat inap dari IGD atau Poliklinik) dan sampai pada Langkah 5 (Pilih Bed), petugas tidak melihat visual denah kamar RS (Lantai 2, 3, 4, 5, 8, 9). Petugas hanya melihat formulir berisi kotak dropdown *"Tempat Tidur \*"* dengan daftar nama kamar dan bed teks biasa.

**Kenapa terjadi.**  
Pada berkas [inpatient-admission-bed-step.jsx](file:///c:/Users/Asoiii/Documents/Quilvian/SourceCode/QuilvianFinal/QuilvianSystemFrontendDev/src/components/view/health-services/inpatient-management/inpatient-admission-bed-step.jsx#L128-L184):
```jsx
// Baris 128:
{episodeGuardMessage ? null : selectionMode === "floor-plan" ? (
  // Render Denah FloorPlanSelection
) : (
  <section aria-label="Pilihan bed admisi transfer">
    <InformationAlert
      variant="info"
      title="Pilihan bed admisi transfer"
      message="Alur transfer dari IGD/Poli membuat episode dan reservasi secara atomik. Pilihan di bawah hanya memuat bed yang diloloskan server dan tidak menampilkan papan kartu lama."
    />
    <BaseSelectField
      field={{
        name: "transferAdmissionBedId",
        label: "Tempat Tidur",
        placeholder: "Pilih bed yang tersedia",
        required: true,
      }}
      value={selectedBedId}
      options={transferBedOptions}
      loading={board?.loading}
      disabled={bed.busy}
      emptyText="Tidak ada bed yang tersedia."
      onChange={selectTransferBed}
    />
    ...
  </section>
)}
```
Kondisi `selectionMode` ditentukan oleh hook [use-inpatient-admission-bed.jsx](file:///c:/Users/Asoiii/Documents/Quilvian/SourceCode/QuilvianFinal/QuilvianSystemFrontendDev/src/lib/hooks/health-services/inpatient-management/use-inpatient-admission-bed.jsx#L122):
```javascript
// Baris 122:
const usesAdmissionFloorPlan = Boolean(admission && !transfer);

// Baris 823:
selectionMode: usesAdmissionFloorPlan ? "floor-plan" : "board",
```
Karena parameter `transfer` aktif (berisi konfigurasi transfer disposisi IGD), `usesAdmissionFloorPlan` dievaluasi menjadi `false`, sehingga `selectionMode` menjadi `"board"` dan hook denah `useFloorPlan` dinonaktifkan.

**Apakah ini menyimpang dari desain?**  
Ya, secara historis merupakan *scope gap*. Pada laporan [FE-RWI-248.md](file:///c:/Users/Asoiii/Documents/Quilvian/SourceCode/QuilvianFinal/NewQuilvianSystemBackend/docs/module-blueprints/rawat-inap/episode-rawat-inap/task/report/frontend/FE-RWI-248.md#L48):
> *"inpatient-admission-bed-step.jsx mengganti board kartu dengan denah hanya untuk Admission; jalur Transfer mempertahankan board existing sampai FE-RWI-249"*
> *"Board lama pada Transfer: KEEP UNTIL FE-RWI-249"*

Namun saat task `FE-RWI-249` diimplementasikan, task tersebut hanya menyasar *Internal Bed Transfer* (pindah tempat tidur antar-ruangan untuk pasien yang sudah dirawat inap), bukan *Admisi Transfer Pasien Masuk dari IGD/Poli*. Akibatnya alur admisi transfer terlewat dan tetap menggunakan kontrol dropdown sementara.

**Dampak nyata.**  
Petugas admisi yang menerima pasien rujukan dari IGD tidak dapat melihat tata letak fisik kamar, posisi ranjang yang kosong, atau kamar yang bersebelahan dengan pasien isolasi secara spasial di layar denah. Petugas harus membaca daftar panjang teks di dropdown, yang menyulitkan pengambilan keputusan penempatan pasien.

**Rekomendasi.**  
Ubah `InpatientAdmissionBedSelectionStep` dan `useInpatientAdmissionBed` agar jalur transfer juga menggunakan komponen visual `FloorPlanSelection` dengan status keterpilihan (*selectable*) disinkronisasikan dari ketersediaan bed server.

---

### ISS-EPS-024-02 — Kontrak `GET /floor-plan` backend mewajibkan `EpisodeId`, padahal alur transfer belum memiliki episode di langkah 5

| Field | Nilai |
| :--- | :--- |
| **No. laporan** | 3 |
| **Jenis** | `CONTRACT_MISMATCH` |
| **Area** | Backend / Frontend Adapter |
| **Keparahan** | `High` |
| **Status bukti** | `SUDAH-VERIFIKASI` di source code backend C# |
| **Perbaikan** | `FIX-EPS-024-02` |

**Apa yang terjadi.**  
Frontend tidak bisa langsung menyalakan pemanggilan `useFloorPlan({ purpose: "Admission", episodeId })` pada jalur transfer, karena jika dipanggil tanpa `episodeId`, backend melempar galat HTTP 400 Bad Request.

**Kenapa terjadi.**  
Di backend C# [InpFloorPlanQueryService.cs](file:///c:/Users/Asoiii/Documents/Quilvian/SourceCode/QuilvianFinal/NewQuilvianSystemBackend/Areas/HealthServices/InPatientManagement/Services/InpFloorPlanQueryService.cs#L73-L97):
```csharp
public async Task<FloorPlanResponse> GetSelectionAsync(FloorPlanSelectionQuery? query, CancellationToken ct)
{
    var purpose = query?.Purpose?.Trim();
    if (!string.Equals(purpose, "Admission", StringComparison.OrdinalIgnoreCase) &&
        !string.Equals(purpose, "Transfer", StringComparison.OrdinalIgnoreCase))
        throw new InpBedOperationException(400, "INVALID_INPUT", "Tujuan denah harus Admission atau Transfer.");
    
    // WAJIB EpisodeId:
    if (query!.EpisodeId is not { } episodeId || episodeId == Guid.Empty)
        throw new InpBedOperationException(400, "INVALID_INPUT", "Episode wajib dikirim dari konteks layar.");

    var isTransfer = string.Equals(purpose, "Transfer", StringComparison.OrdinalIgnoreCase);

    await access.RequireEpisodeAsync(episodeId, SelectionOperation, ct);
    var episode = await db.Set<InpEpisode>().AsNoTracking().FirstOrDefaultAsync(x => x.Id == episodeId && !x.IsDelete, ct)
        ?? throw new InpBedOperationException(404, "NOT_FOUND", "Episode rawat inap tidak ditemukan.");

    if (isTransfer)
    {
        // Hanya untuk transfer kamar internal pasien Admitted:
        var activePlacement = episode.EpisodeStatus == InpEpisodeStatus.Admitted
            ? await db.Set<InpBedPlacement>().AsNoTracking()
                .FirstOrDefaultAsync(x => x.EpisodeId == episode.Id && x.EndDateTime == null && !x.IsDelete, ct)
            : null;
        if (activePlacement == null)
            throw new InpBedOperationException(422, "FLOOR_PLAN_PURPOSE_MISMATCH",
                "Denah tujuan transfer hanya untuk pasien yang sedang dirawat dan menempati bed. Kembali ke langkah sebelumnya.");
    }
    else if (episode.EpisodeStatus != InpEpisodeStatus.Draft)
    {
        throw new InpBedOperationException(422, "FLOOR_PLAN_PURPOSE_MISMATCH",
            "Denah Pilih Bed admisi hanya untuk admisi yang masih disiapkan. Kembali ke langkah sebelumnya.");
    }
    ...
```

Pada alur transfer IGD/Poli:
- Pasien dirujuk lewat disposisi rawat inap IGD (`transferReferral`).
- Sesuai prinsip atomik transfer [ISS-EPS-006-01](file:///c:/Users/Asoiii/Documents/Quilvian/SourceCode/QuilvianFinal/NewQuilvianSystemBackend/docs/module-blueprints/rawat-inap/episode-rawat-inap/docs/issue/issue-006-admisi-transfer-igd-rawat-jalan.md#L88), Kunjungan (`RegPatientEncounter`), Episode (`InpEpisode`), dan Reservasi Bed dibuat bersamaan secara atomik saat submit di Langkah 6 melalui endpoint `POST /admission-transfers/admit`.
- Oleh karena itu, pada Langkah 5 (Pilih Bed), **belum ada `InpEpisode` di database** (statusnya belum tersimpan).
- Akibatnya:
  - Mengirim `purpose: "Admission"` akan gagal HTTP 400 karena `EpisodeId` belum ada.
  - Mengirim `purpose: "Transfer"` akan gagal HTTP 422 karena pasien belum memiliki penempatan tempat tidur aktif (`InpBedPlacement`).

**Dampak nyata.**  
Frontend tidak bisa serta-merta mengganti parameter pemanggilan `useFloorPlan` tanpa menangani dependensi `episodeId` ini. Bila dipaksa tanpa penyesuaian adapter, layar Langkah 5 akan langsung menampilkan galat merah penolakan server.

**Rekomendasi.**  
Gunakan **Pola Adapter Frontend (Overlay Ketersediaan)**:
1. Frontend memanggil denah tata ruang per lantai menggunakan service denah umum `bedManagementService.getFloorPlan(floorCode)` yang tidak menuntut `episodeId`.
2. Frontend melakukan pemetaan kelayakan tempat tidur (*overlay eligibility*) dengan mencocokkan data tempat tidur yang sudah diloloskan oleh server pada endpoint `GET /bed-occupancies/available-beds` (yang sudah dipanggil oleh `useInpatientBedBoard`).
3. Bed yang tercatat di `board.availableBeds` ditandai sebagai `isSelectable: true` di denah dan dapat diklik oleh pengguna.
4. Ketika bed di denah diklik, data `selectedBed` (termasuk `operationalVersion` dan `patientClassId`) disimpan ke state, dan alur lanjut ke Langkah 6 untuk mengeksekusi `admitFromTransfer` secara atomik tanpa mengubah integritas backend.

---

## 5. Tanya-Jawab Pelapor

> **T:** Kenapa pada pilih bed di menu admisi transfer pasien belum bisa tampil bed denah?  
> **J:** Karena pada saat fitur denah diimplementasikan di task `FE-RWI-248`, integrasi denah difokuskan pada pendaftaran pasien reguler dan bayi, sedangkan jalur transfer IGD/Poli sengaja ditahan sementara dan dibiarkan memakai dropdown. Selain itu, alur transfer IGD belum membuat nomor episode di database pada langkah tersebut, sedangkan endpoint denah server mewajibkan nomor episode rawat inap aktif.

> **T:** Apakah alur transfer bisa menggunakan denah yang sama seperti menu admisi biasa?  
> **J:** Ya, sangat bisa. Denah visualnya (Lantai 2, 3, 4, 5, 8, 9) memakai komponen geometri yang sama persis (`FloorPlanSelection`). Perbedaannya hanya pada cara frontend menentukan bed mana yang boleh dipilih: pada jalur transfer, bed yang dapat dipilih disinkronkan langsung dari daftar tempat tidur yang sudah diverifikasi ketersediaannya oleh server (`available-beds`).

---

## 6. Temuan Tambahan

| Field | Nilai |
| :--- | :--- |
| **Kenapa dicantumkan** | Ditemukan saat penelusuran alur Langkah 6 (Booking Bed) untuk transfer |
| **Temuan** | Pada Langkah 6 (Booking Bed), kartu ringkasan bed terpilih (`SelectedBedCard`) sudah siap menerima objek bed dari denah dan tombol *"Pesan Tempat Tidur"* sudah terhubung dengan `transfer.submit(effectiveSelectedBed)` yang mengeksekusi `admitFromTransfer`. Artinya, jalur eksekusi di hilir sudah siap dan aman; hanya antarmuka pemilihan di Langkah 5 yang tertinggal. |

---

## 7. Pertanyaan dan Keputusan yang Dibutuhkan

### Untuk Pelapor
*Tidak ada pertanyaan yang menahan analisis. Masalah dan kebutuhan pengguna sudah sangat jelas dan teridentifikasi akar penyebabnya.*

### Untuk Pemilik Sistem

| No | Keputusan | Pilihan Solusi | Rekomendasi | Menahan Perbaikan |
| :---: | :--- | :--- | :--- | :--- |
| **K-01** | Pendekatan integrasi denah pada alur transfer admisi | **Opsi 1 (Frontend Adapter Pattern):** Menggunakan denah denah tata letak bersama (`FloorPlanSelection`) dengan *overlay* status kelayakan dari `availableBeds` yang sudah diloloskan server.<br><br>**Opsi 2 (Backend Contract Extension):** Mengubah kontrak backend `GET /bed-occupancies/floor-plan` agar menerima query ketersediaan denah berbasis disposisi transfer tanpa mewajibkan `EpisodeId`. | **Opsi 1 (Rekomendasi Utama)**, karena dapat langsung diterapkan di frontend secara aman, tidak memerlukan migrasi/perubahan backend C#, dan tetap menjaga integritas transaksi atomik transfer `admitFromTransfer`. | `FIX-EPS-024-01` |

---

## 8. Catatan Pola

Masalah ini adalah kelanjutan dari penutupan celah antarmuka (*UI Gap Closure*) pada transisi dari sistem lama ke sistem berbasis denah visual (`FE-RWI-248` dan `FE-RWI-249`). Standarisasi seluruh interaksi penempatan bed ke komponen `FloorPlanSelection` merupakan langkah penting agar staf admisi RS MMC tidak perlu berganti-ganti mental model antara alur biasa (pakai denah) dan alur transfer (pakai dropdown).

---

## 9. Riwayat Dokumen

| Tanggal | Perubahan | Oleh |
| :--- | :--- | :--- |
| 2026-10-17 | Pembuatan dokumen issue awal berdasarkan analisis source code frontend dan backend serta screenshot pengguna | Antigravity AI (`diagnose-module-issue`) |
| 2026-10-17 | Pemilik menyetujui rencana perbaikan Opsi A; status beralih ke DALAM_PERBAIKAN | Pemilik Sistem (User) |
| 2026-10-17 | Perbaikan selesai diimplementasikan dan diverifikasi lewat unit test `floor-plan-transfer-admission.test.mjs` (Status: SELESAI) | Antigravity AI (`build-module-frontend`) |
